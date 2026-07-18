using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Media;
using TOTP.Avalonia.Desktop.Platform;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class AccountListViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IAccountManager _accountManager;
    private readonly IAccountTotpService _accountTotpService;
    private readonly IAsyncClipboardService _clipboardService;
    private readonly IAccountQrCodeService _accountQrCodeService;
    private readonly IAvaloniaQrImageFactory _qrImageFactory;
    private readonly AsyncCommand _loadCommand;
    private readonly AsyncCommand _generateCommand;
    private readonly AsyncCommand _copyCommand;
    private readonly AsyncCommand _generateQrCommand;
    private CancellationTokenSource? _codeLifetime;
    private IReadOnlyList<AccountListItemViewModel> _allAccounts = [];
    private IReadOnlyList<AccountListItemViewModel> _accounts = [];
    private string _message = string.Empty;
    private string _searchText = string.Empty;
    private AccountListItemViewModel? _selectedAccount;
    private string _generatedCode = string.Empty;
    private string _codeMessage = string.Empty;
    private bool _isBusy;
    private bool _isGenerating;
    private int _remainingSeconds;
    private AvaloniaQrImageHandle? _qrImage;

    public AccountListViewModel(
        IAccountManager accountManager,
        IAccountTotpService accountTotpService,
        IAsyncClipboardService clipboardService,
        IAccountQrCodeService accountQrCodeService,
        IAvaloniaQrImageFactory qrImageFactory)
    {
        _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
        _accountTotpService = accountTotpService ?? throw new ArgumentNullException(nameof(accountTotpService));
        _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        _accountQrCodeService = accountQrCodeService ?? throw new ArgumentNullException(nameof(accountQrCodeService));
        _qrImageFactory = qrImageFactory ?? throw new ArgumentNullException(nameof(qrImageFactory));
        _loadCommand = new AsyncCommand(LoadAsync, () => !_isBusy);
        _generateCommand = new AsyncCommand(
            GenerateCodeAsync,
            () => !_isGenerating && _selectedAccount is not null);
        _copyCommand = new AsyncCommand(CopyCodeAsync, () => HasGeneratedCode);
        _generateQrCommand = new AsyncCommand(GenerateQrAsync, () => _selectedAccount is not null);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<AccountListItemViewModel> Accounts
    {
        get => _accounts;
        private set => SetField(ref _accounts, value);
    }

    public string Message
    {
        get => _message;
        private set
        {
            if (!SetField(ref _message, value)) return;
            OnPropertyChanged(nameof(HasMessage));
        }
    }

    public bool HasMessage => Message.Length > 0;

    public AccountListItemViewModel? SelectedAccount
    {
        get => _selectedAccount;
        set
        {
            if (!SetField(ref _selectedAccount, value)) return;
            ClearGeneratedCode();
            ClearQrImage();
            _generateCommand.NotifyCanExecuteChanged();
            _generateQrCommand.NotifyCanExecuteChanged();
        }
    }

    public string GeneratedCode
    {
        get => _generatedCode;
        private set
        {
            if (!SetField(ref _generatedCode, value)) return;
            OnPropertyChanged(nameof(HasGeneratedCode));
            _copyCommand.NotifyCanExecuteChanged();
        }
    }

    public bool HasGeneratedCode => GeneratedCode.Length > 0;

    public string CodeMessage
    {
        get => _codeMessage;
        private set
        {
            if (!SetField(ref _codeMessage, value)) return;
            OnPropertyChanged(nameof(HasCodeMessage));
        }
    }

    public bool HasCodeMessage => CodeMessage.Length > 0;

    public IImage? QrImage => _qrImage?.Image;

    public bool HasQrImage => QrImage is not null;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (!SetField(ref _searchText, value ?? string.Empty)) return;
            ApplyFilter();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetField(ref _isBusy, value)) return;
            _loadCommand.NotifyCanExecuteChanged();
        }
    }

    public ICommand LoadCommand => _loadCommand;

    public ICommand GenerateCommand => _generateCommand;

    public ICommand CopyCommand => _copyCommand;

    public ICommand GenerateQrCommand => _generateQrCommand;

    public async Task LoadAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        Message = string.Empty;
        try
        {
            var result = await _accountManager.GetAllOtpEntriesSortedAsync();
            if (result.IsFailed)
            {
                _allAccounts = [];
                Accounts = [];
                Message = "Accounts could not be loaded. Your encrypted data was not changed.";
                return;
            }

            _allAccounts = result.Value
                .Select(account => new AccountListItemViewModel(
                    account.ID,
                    account.Issuer,
                    account.AccountName ?? string.Empty))
                .ToArray();
            ApplyFilter();
        }
        catch (Exception)
        {
            _allAccounts = [];
            Accounts = [];
            Message = "Accounts could not be loaded safely. Try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task GenerateCodeAsync()
    {
        if (_selectedAccount is null || _isGenerating) return;

        _isGenerating = true;
        _generateCommand.NotifyCanExecuteChanged();
        ClearGeneratedCode();
        try
        {
            var result = await _accountTotpService.GenerateAsync(_selectedAccount.Id);
            if (result.IsFailed)
            {
                CodeMessage = "A code could not be generated for this account.";
                return;
            }

            GeneratedCode = result.Value.Code;
            _remainingSeconds = Math.Max(1, result.Value.RemainingSeconds);
            CodeMessage = $"Valid for {result.Value.RemainingSeconds} seconds.";
            _codeLifetime = new CancellationTokenSource();
            _ = ClearGeneratedCodeAfterAsync(
                TimeSpan.FromSeconds(Math.Max(1, result.Value.RemainingSeconds)),
                _codeLifetime.Token);
        }
        catch (Exception)
        {
            CodeMessage = "A code could not be generated safely. Try again.";
        }
        finally
        {
            _isGenerating = false;
            _generateCommand.NotifyCanExecuteChanged();
        }
    }

    public async Task CopyCodeAsync()
    {
        if (!HasGeneratedCode) return;

        var result = await _clipboardService.CopyAndScheduleClearAsync(
            GeneratedCode,
            TimeSpan.FromSeconds(_remainingSeconds));
        CodeMessage = result.IsSuccess
            ? $"Copied. Clipboard clear is scheduled in {_remainingSeconds} seconds."
            : "This platform cannot safely copy and automatically clear the code.";
    }

    public async Task GenerateQrAsync()
    {
        if (_selectedAccount is null) return;

        ClearQrImage();
        var result = await _accountQrCodeService.GenerateAsync(_selectedAccount.Id);
        if (result.IsFailed)
        {
            CodeMessage = "A QR code could not be generated for this account.";
            return;
        }

        using var sensitivePng = result.Value;
        try
        {
            _qrImage = _qrImageFactory.Create(sensitivePng.Memory);
            OnPropertyChanged(nameof(QrImage));
            OnPropertyChanged(nameof(HasQrImage));
        }
        catch (Exception)
        {
            ClearQrImage();
            CodeMessage = "A QR code could not be displayed safely.";
        }
    }

    private void ApplyFilter()
    {
        Accounts = AccountListFilter.Apply(_allAccounts, SearchText);
    }

    private async Task ClearGeneratedCodeAfterAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken);
            GeneratedCode = string.Empty;
            CodeMessage = "Code expired. Generate a new code.";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private void ClearGeneratedCode()
    {
        _codeLifetime?.Cancel();
        _codeLifetime?.Dispose();
        _codeLifetime = null;
        GeneratedCode = string.Empty;
        _remainingSeconds = 0;
        CodeMessage = string.Empty;
    }

    public void Dispose()
    {
        ClearGeneratedCode();
        ClearQrImage();
    }

    public void Clear()
    {
        SelectedAccount = null;
        SearchText = string.Empty;
        _allAccounts = [];
        Accounts = [];
        Message = string.Empty;
        ClearGeneratedCode();
        ClearQrImage();
    }

    private void ClearQrImage()
    {
        var image = _qrImage;
        _qrImage = null;
        OnPropertyChanged(nameof(QrImage));
        OnPropertyChanged(nameof(HasQrImage));
        image?.Dispose();
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
