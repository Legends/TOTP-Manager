using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FluentResults;
using TOTP.Avalonia.Desktop.Localization;
using TOTP.Avalonia.Desktop.Platform;
using TOTP.Avalonia.Desktop.Presentation.Dialogs;
using TOTP.Core.Common;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Services.Models;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class NativeFilePickerViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IAvaloniaFilePicker _filePicker;
    private readonly IExportService _exportService;
    private readonly IAccountManager _accountManager;
    private readonly IAccountImportService _accountImportService;
    private readonly IAvaloniaDialogService _dialogs;
    private readonly IPasswordValidationService _passwordValidation;
    private readonly IPlatformFileSecurity _fileSecurity;
    private readonly ISettingsService _settings;
    private readonly IPlatformFolderLauncher _folderLauncher;
    private readonly IAvaloniaLocalizationService _localization;
    private readonly AsyncCommand _importCommand;
    private readonly AsyncCommand _exportCommand;
    private bool _isBusy;
    private bool _disposed;
    private ImportConflictStrategy _conflictStrategy = ImportConflictStrategy.SkipExisting;
    private ImportStrategyOption? _selectedConflictStrategyOption;

    public NativeFilePickerViewModel(
        IAvaloniaFilePicker filePicker,
        IExportService exportService,
        IAccountManager accountManager,
        IAccountImportService accountImportService,
        IAvaloniaDialogService dialogs,
        IPasswordValidationService passwordValidation,
        IPlatformFileSecurity fileSecurity,
        ISettingsService settings,
        IPlatformFolderLauncher folderLauncher,
        IAvaloniaLocalizationService localization,
        TimeSpan? transientMessageDuration = null)
    {
        _filePicker = filePicker ?? throw new ArgumentNullException(nameof(filePicker));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
        _accountImportService = accountImportService ?? throw new ArgumentNullException(nameof(accountImportService));
        _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        _passwordValidation = passwordValidation ?? throw new ArgumentNullException(nameof(passwordValidation));
        _fileSecurity = fileSecurity ?? throw new ArgumentNullException(nameof(fileSecurity));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _folderLauncher = folderLauncher ?? throw new ArgumentNullException(nameof(folderLauncher));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        Notification = new NotificationState(transientMessageDuration);
        ConflictStrategies = CreateConflictStrategies();
        _selectedConflictStrategyOption = ConflictStrategies[0];
        _localization.CultureChanged += LocalizationCultureChanged;
        _importCommand = new AsyncCommand(ImportAsync, () => !_isBusy);
        _exportCommand = new AsyncCommand(ExportEncryptedAsync, () => !_isBusy);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? AccountsChanged;

    public IReadOnlyList<ImportStrategyOption> ConflictStrategies { get; private set; }

    public ImportConflictStrategy ConflictStrategy
    {
        get => _conflictStrategy;
        set => SetField(ref _conflictStrategy, value);
    }

    public ImportStrategyOption? SelectedConflictStrategyOption
    {
        get => _selectedConflictStrategyOption;
        set
        {
            if (!SetField(ref _selectedConflictStrategyOption, value) || value is null) return;
            ConflictStrategy = value.Strategy;
        }
    }

    public NotificationState Notification { get; }
    public string Message => Notification.Text;
    public NotificationSeverity MessageSeverity => Notification.Severity;

    public ICommand ImportCommand => _importCommand;
    public ICommand ExportCommand => _exportCommand;

    public async Task ImportAsync()
    {
        if (!BeginOperation()) return;
        try
        {
            await using var file = await _filePicker.PickImportFileAsync();
            if (file is null)
            {
                ShowTransientMessage(
                    _localization.GetString(AvaloniaStringKeys.NoImportFileSelected),
                    NotificationSeverity.Information);
                return;
            }

            var imported = await ReadImportAsync(file);
            if (imported is null) return;
            var importResult = await _accountImportService.ImportAsync(
                imported,
                ConflictStrategy,
                ConfirmImportAsync);
            if (importResult.IsFailed)
            {
                SetMessage(
                    "The import workflow failed safely. No secret details were exposed.",
                    NotificationSeverity.Error);
                return;
            }

            var outcome = importResult.Value;
            if (outcome.Status == AccountImportStatus.Cancelled)
            {
                ShowTransientMessage(OutcomeMessage(outcome), NotificationSeverity.Information);
            }
            else
            {
                SetMessage(
                    OutcomeMessage(outcome),
                    outcome.Status == AccountImportStatus.Completed
                        ? NotificationSeverity.Success
                        : NotificationSeverity.Error);
            }
            if (outcome.Status == AccountImportStatus.Completed
                && outcome.Added + outcome.Replaced > 0)
                AccountsChanged?.Invoke(this, EventArgs.Empty);

            Task<bool> ConfirmImportAsync(AccountImportPreview preview, CancellationToken token) =>
                _dialogs.ConfirmAsync(new ConfirmationDialogRequest(
                    "Import accounts",
                    $"Import {preview.TotalCount} account(s), including {preview.ConflictCount} conflict(s), using '{StrategyLabel(preview.ConflictStrategy)}'? A recovery backup will be created first.",
                    NotificationSeverity.Warning,
                    "Import",
                    "Cancel"), token);
        }
        catch (Exception)
        {
            SetMessage(
                "The import workflow failed safely. No secret details were exposed.",
                NotificationSeverity.Error);
        }
        finally
        {
            EndOperation();
        }
    }

    public async Task ExportEncryptedAsync()
    {
        if (!BeginOperation()) return;
        string? password = null;
        try
        {
            var accounts = await _accountManager.GetAllOtpEntriesSortedAsync();
            if (accounts.IsFailed)
            {
                SetMessage("Accounts could not be loaded for export.", NotificationSeverity.Error);
                return;
            }

            password = await _dialogs.PromptForPasswordAsync(new PasswordDialogRequest(
                "Encrypted export",
                "Create and confirm a password for this portable encrypted backup. This password cannot be recovered.",
                "Continue",
                "Cancel",
                "An export password is required.",
                "The export password could not be validated.",
                (candidate, _) => Task.FromResult<string?>(
                    _passwordValidation.IsValidNew(candidate)
                        ? null
                        : $"Use at least {_passwordValidation.MinimumLength} characters."),
                RequireConfirmation: true,
                ConfirmationRequiredMessage: "Confirm the export password.",
                MismatchMessage: "The export passwords do not match."));
            if (password is null)
            {
                ShowTransientMessage(
                    _localization.GetString(AvaloniaStringKeys.ExportCancelled),
                    NotificationSeverity.Information);
                return;
            }

            var suggestedName = $"totp-backup-{DateTime.UtcNow:yyyy-MM-dd}.totp";
            await using var file = await _filePicker.PickEncryptedExportFileAsync(suggestedName);
            if (file is null)
            {
                ShowTransientMessage(
                    _localization.GetString(AvaloniaStringKeys.NoExportFileSelected),
                    NotificationSeverity.Information);
                return;
            }

            Result result;
            await using (var destination = await file.OpenWriteAsync())
            {
                result = await _exportService.ExportToEncryptedStreamAsync(
                    accounts.Value, password, destination, ExportFileFormat.Json);
            }
            if (result.IsFailed)
            {
                SetMessage(
                    "The encrypted backup could not be written completely. Do not use the selected file as a backup.",
                    NotificationSeverity.Error);
                return;
            }

            if (file.LocalPath is { } localPath)
            {
                try
                {
                    _fileSecurity.RestrictFileToCurrentUser(localPath);
                }
                catch (Exception)
                {
                    SetMessage(
                        "The backup is encrypted, but its local file permissions could not be verified.",
                        NotificationSeverity.Warning);
                    return;
                }

                if (_settings.Current.OpenExportFileAfterExport
                    && Path.GetDirectoryName(localPath) is { } folder)
                {
                    await _folderLauncher.OpenFolderAsync(folder);
                }
            }

            SetMessage(
                $"Encrypted backup '{file.Name}' created successfully.",
                NotificationSeverity.Success);
        }
        catch (Exception)
        {
            SetMessage(
                "The encrypted export failed safely. Do not use an incomplete output file.",
                NotificationSeverity.Error);
        }
        finally
        {
            password = null;
            EndOperation();
        }
    }

    private async Task<List<Account>?> ReadImportAsync(INativeStorageFile file)
    {
        Result<List<Account>> result;
        if (Path.GetExtension(file.Name).Equals(".totp", StringComparison.OrdinalIgnoreCase))
        {
            Result<List<Account>>? validatedResult = null;
            var password = await _dialogs.PromptForPasswordAsync(new PasswordDialogRequest(
                "Import encrypted backup",
                "Enter the password used when this backup was created.",
                "Unlock backup",
                "Cancel",
                "The import password is required.",
                "The backup could not be validated safely.",
                async (candidate, token) =>
                {
                    await using var stream = await file.OpenReadAsync(token);
                    validatedResult = await _exportService.ImportFromStreamAsync(
                        stream, file.Name, candidate, token);
                    return validatedResult.GetErrorCode() == AppErrorCode.ImportWrongPasswordOrTampered
                        ? "The password is incorrect or the backup was modified."
                        : validatedResult.IsFailed
                            ? "The backup is invalid or unavailable."
                            : null;
                }));
            password = null;
            if (validatedResult is null)
            {
                ShowTransientMessage(
                    _localization.GetString(AvaloniaStringKeys.ImportCancelled),
                    NotificationSeverity.Information);
                return null;
            }

            result = validatedResult;
        }
        else
        {
            await using var stream = await file.OpenReadAsync();
            result = await _exportService.ImportFromStreamAsync(stream, file.Name);
        }

        if (result.IsFailed)
        {
            SetMessage(
                "The selected import file is invalid, unavailable, or unsupported.",
                NotificationSeverity.Error);
            return null;
        }

        if (result.Value.Count == 0)
        {
            SetMessage("The selected file contains no accounts.", NotificationSeverity.Warning);
            return null;
        }

        return result.Value;
    }

    private static string OutcomeMessage(AccountImportOutcome outcome) => outcome.Status switch
    {
        AccountImportStatus.Completed =>
            $"Import complete: {outcome.Added} added, {outcome.Replaced} replaced, {outcome.Skipped} skipped, {outcome.Failed} failed.",
        AccountImportStatus.Cancelled => "Import cancelled. No data was changed.",
        AccountImportStatus.InvalidTargets =>
            "The import contains invalid or excessive account data. No data was changed.",
        AccountImportStatus.ExistingAccountsUnavailable =>
            "Existing accounts could not be loaded. No data was changed.",
        AccountImportStatus.RecoveryBackupFailed =>
            "A recovery backup could not be created. Import was stopped before changing data.",
        _ => "The import workflow could not be completed safely."
    };

    private bool BeginOperation()
    {
        if (_isBusy) return false;
        _isBusy = true;
        Notification.Clear();
        _importCommand.NotifyCanExecuteChanged();
        _exportCommand.NotifyCanExecuteChanged();
        return true;
    }

    private void EndOperation()
    {
        _isBusy = false;
        _importCommand.NotifyCanExecuteChanged();
        _exportCommand.NotifyCanExecuteChanged();
    }

    private string StrategyLabel(ImportConflictStrategy strategy) =>
        ConflictStrategies.First(option => option.Strategy == strategy).Label;

    private IReadOnlyList<ImportStrategyOption> CreateConflictStrategies() =>
    [
        new(ImportConflictStrategy.SkipExisting, _localization.GetString(AvaloniaStringKeys.ImportSkipExisting)),
        new(ImportConflictStrategy.ReplaceExisting, _localization.GetString(AvaloniaStringKeys.ImportReplaceExisting)),
        new(ImportConflictStrategy.KeepBoth, _localization.GetString(AvaloniaStringKeys.ImportKeepBoth))
    ];

    private void LocalizationCultureChanged(object? sender, EventArgs e)
    {
        var selectedStrategy = ConflictStrategy;
        ConflictStrategies = CreateConflictStrategies();
        OnPropertyChanged(nameof(ConflictStrategies));
        SelectedConflictStrategyOption = ConflictStrategies.First(option => option.Strategy == selectedStrategy);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _localization.CultureChanged -= LocalizationCultureChanged;
        Notification.Dispose();
    }

    private void SetMessage(string message, NotificationSeverity severity)
        => Notification.ShowPersistent(message, severity);

    private void ShowTransientMessage(string message, NotificationSeverity severity)
        => Notification.ShowTransient(message, severity);

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public sealed record ImportStrategyOption(ImportConflictStrategy Strategy, string Label);

}
