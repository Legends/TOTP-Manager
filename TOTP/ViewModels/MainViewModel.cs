using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TOTP.Commands;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Events;
using TOTP.Helper;
using TOTP.Interfaces;
using TOTP.Resources;
using TOTP.Services;

namespace TOTP.ViewModels;

public class MainViewModel : IMainViewModel, INotifyPropertyChanged //, ILocalizable
{
    #region ### PROPERTIES AND VARS ###

    private CultureDisplay _selectedCulture;
    private readonly ILogger<MainViewModel> _logger;
    private readonly IPlatformSecretDialogService _platformSecretDialogService;
    public ObservableCollection<CultureDisplay> SupportedCultures { get; }

    public CultureDisplay SelectedCulture
    {
        get => _selectedCulture;
        set
        {
            if (_selectedCulture != value)
            {
                _selectedCulture = value;
                LocalizationService.ChangeCulture(value.Culture.Name);

                OnPropertyChanged();
                //LanguageChanged?.Invoke(this, value.Culture);
            }
        }
    }

    private int _codeLabelHeight;

    public int CodeLabelHeight
    {
        get => _codeLabelHeight;
        set
        {
            _codeLabelHeight = value;
            OnPropertyChanged();
        }
    }


    private BitmapImage? _qrCodeImage;

    public BitmapImage? QrCodeImage
    {
        get => _qrCodeImage;
        set
        {
            _qrCodeImage = value;
            OnPropertyChanged();
        }
    }

    private bool _isSearchFocused;

    public bool IsSearchFocused
    {
        get => _isSearchFocused;
        set
        {
            _isSearchFocused = value;
            OnPropertyChanged();
        }
    }

    public bool IsContextmenuOpen { get; set; }

    public bool ShowActionsColumn => AllSecrets.Any(s => s.IsBeingEdited);

    private SecretItemViewModel _selectedSecret = null!;

    public SecretItemViewModel SelectedSecret
    {
        get => _selectedSecret;
        set
        {
            if (!EqualityComparer<SecretItemViewModel?>.Default.Equals(_selectedSecret, value))
            {
                foreach (var item in AllSecrets)
                    item.IsBeingEdited = false;

                _selectedSecret = value;
                OnPropertyChanged();
                //OnSecretSelectedAsync();
            }
        }
    }

    private string _currentCodeLabel = string.Empty;

    public string CurrentCodeLabel
    {
        get => _currentCodeLabel;
        set
        {
            _currentCodeLabel = value;
            OnPropertyChanged();
        }
    }


    public SecretItemViewModel? PreviousVersion { get; set; }

    private bool _isSearchVisible;

    public bool IsSearchVisible
    {
        get => _isSearchVisible;
        set
        {
            _isSearchVisible = value;
            OnPropertyChanged();
        }
    }

    private bool _isCodeCopiedVisible;

    public bool IsCodeCopiedVisible
    {
        get => _isCodeCopiedVisible;
        set
        {
            _isCodeCopiedVisible = value;
            OnPropertyChanged();
        }
    }

    private bool _ssQrVisible;

    public bool IsQrVisible
    {
        get => _ssQrVisible;
        set
        {
            _ssQrVisible = value;
            OnPropertyChanged();
        }
    }

    private bool _isCodeLabelVisible;

    public bool IsCodeLabelVisible
    {
        get => _isCodeLabelVisible;
        set
        {
            _isCodeLabelVisible = value;
            OnPropertyChanged();
        }
    }


    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set
        {
            ResetCodeGenerationLabels();
            _searchText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSearchTextNotEmpty));
            _debounceService.Debounce("Search", 300, () => ExecuteSearch());
        }
    }

    public bool IsSearchTextNotEmpty => !string.IsNullOrEmpty(_searchText);

    #endregion REGION PROPERTIES AND VARS

    #region ### Events ###
    //public event EventHandler<CultureInfo>? LanguageChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region ### Constructor ###

    public MainViewModel(
        IPlatformSecretDialogService platformSecretDialogService,
        ILogger<MainViewModel> logger,
        IQrCodeService svcQr,
        IMessageService messageService,
        IClipboardService clipboard,
        IConfiguration config,
        ITotpManager totpManager,
        IDebounceService debounceService,
        IDelayService delayService,
        ISecretsManager secretsManager) // NEW
    {
        _platformSecretDialogService = platformSecretDialogService;
        _secretsManager = secretsManager;
        _logger = logger;
        _qrService = svcQr;
        _delayService = delayService;
        _messageService = messageService;
        _debounceService = debounceService;
        _clipboard = clipboard;
        _totpManager = totpManager;

        _totpManager.OnMessageSend += _totpManager_OnMessageSend;
        _totpManager.OnAddNewPrompt += _totpManager_OnAddNewPrompt;
        _totpManager.ConfirmDeleteRequested += _totpManager_OnDeletePrompt;


        SetupCommandEventhandler();

        SupportedCultures =
        [
            new(new CultureInfo("en"), StringsConstants.ImgUrl.EnFlag),
            new(new CultureInfo("de-DE"), StringsConstants.ImgUrl.DeFlag),
        ];

        var currentCulture = CultureInfo.CurrentUICulture;

        var selCulture = SupportedCultures.FirstOrDefault(c => c.Culture.Name == currentCulture.Name)
                           ?? SupportedCultures.First();
        SelectedCulture = selCulture;
    }


    private bool _totpManager_OnDeletePrompt(object? sender, string platform)
    {
        return _messageService.ShowWarningMessageDialog(string.Format(UI.msg_ConfirmDeleteSecret, platform));
    }

    private AddNewPromptArgs _totpManager_OnAddNewPrompt(object? sender)
    {
        var (success, key, value) = _platformSecretDialogService.ShowForm();
        return new AddNewPromptArgs() { Success = success, Key = key, Value = value };
    }

    private void _totpManager_OnMessageSend(object sender, OperationStatus arg1, string? arg2)
    {
        switch (arg1)
        {
            case OperationStatus.Unknown:
                _messageService.ShowErrorMessage(arg2 ?? "An unknow error has occured");
                break;
            case OperationStatus.NotFound:
                _messageService.ShowErrorMessage($"{UI.msg_Platform_Not_Found}: {arg2}");
                break;
            case OperationStatus.LoadingFailed:
                _messageService.ShowErrorMessage(UI.msg_Failed_Loading_Secrets);
                break;
            case OperationStatus.DeleteFailed:
                break;
            case OperationStatus.UpdateFailed:
                _messageService.ShowErrorMessage($"{UI.msg_Failed_Updating_Secret} : {arg2}");
                break;
            case OperationStatus.CreateFailed:
                _messageService.ShowErrorMessage(string.Format(UI.msg_FailedAddingSecret, arg2 ?? ""));
                break;
            case OperationStatus.StorageFailed:
                _messageService.ShowErrorMessage($"{UI.msg_Failed_Storage}: {arg2 ?? ""}");
                break;
            case OperationStatus.Success:
                _messageService.ShowInfoMessage($"{UI.msg_SecretUpdated}: {arg2}");
                break;
            case OperationStatus.AlreadyExists:
                _messageService.ShowErrorMessage(string.Format(UI.msg_Platform_Exists, arg2));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(arg1), arg1, null);
        }
    }

    public async Task InitializeAsync()
    {
        await ReadAllSecretsAsync();
        //OnPropertyChanged(nameof(ShowActionsColumn));
        UpdateSearchFilter();
    }


    #endregion

    #region ### COMMANDS EVENTHANDLER ###

    private void SetupCommandEventhandler()
    {
        AddNewSecretCommand = new AsyncCommand(AddNewSecretAsync, null, _logger);
        DeleteSecretCommand = new AsyncCommand<SecretItemViewModel>(DeleteSecretAsync, null, _logger);
        UpdateSecretCommand = new AsyncCommand<SecretItemViewModel>(UpdateSecretAsync, null, _logger);
        BeginEditCommand = new RelayCommand<SecretItemViewModel>(OnBeginEdit);
        EndEditCommand = new AsyncCommand<SecretItemViewModel>(OnEndEdit); // Method must be: Task OnEndEditAsync()
        //SelectionChangedCommand = new AsyncCommand(async _ => await OnSelectionChangedAsync());
        SelectionChangedCommand = new AsyncCommand(OnSelectionChangedAsync);



        DoubleClickCommand = new RelayCommand<SecretItemViewModel>(OnDoubleClick);

        ToggleSearchBoxCommand = new RelayCommand(() =>
        {
            IsSearchVisible = !IsSearchVisible;
            IsSearchFocused = IsSearchVisible;
        });

        ClearSearchCommand = new RelayCommand(() =>
        {
            SearchText = "";

            // the property doesnt change if IsSearchFocused is already true
            // so, setting true => true doesnt raise onpropertyChanged and therefore no focus occurs
            // A common pattern is to first set it to false, then back to true,
            // to force the property changed notification:
            IsSearchFocused = false;
            IsSearchFocused = IsSearchVisible;
        });
    }

    #endregion COMMANDS SETUP

    private void SecretItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SecretItemViewModel.IsBeingEdited))
            OnPropertyChanged(nameof(ShowActionsColumn));
    }

    #region ### ObservableCollections ###

    public ObservableCollection<SecretItemViewModel> AllSecrets { get; private set; } = [];
    public ObservableCollection<SecretItemViewModel> FilteredSecrets { get; } = [];

    #endregion ObservableCollections

    #region ### COMMANDS DECLARATION ###

    public ICommand ChangeLanguageCommand { get; private set; } = null!;
    public ICommand AddNewSecretCommand { get; private set; } = null!;
    public ICommand BeginEditCommand { get; private set; } = null!;
    public ICommand ClearSearchCommand { get; private set; } = null!;
    public ICommand DeleteSecretCommand { get; private set; } = null!;
    public ICommand DoubleClickCommand { get; private set; } = null!;
    public ICommand EndEditCommand { get; private set; } = null!;
    public ICommand ToggleSearchBoxCommand { get; private set; } = null!;
    public ICommand UpdateSecretCommand { get; private set; } = null!;

    public ICommand SelectionChangedCommand { get; private set; } = null!;

    #endregion REGION COMMANDS

    #region ### SERVICES ###

    private readonly ISecretsManager _secretsManager;
    private readonly IClipboardService _clipboard;
    private readonly IMessageService _messageService;
    private readonly ITotpManager _totpManager;

    private readonly IDebounceService _debounceService;

    //private readonly DispatcherTimer _debounceTimer;
    private readonly IQrCodeService _qrService;

    private readonly IDelayService _delayService;
    //private string _pendingSearchText;

    #endregion REGION SERVICES

    #region ### READ ALL SECRETS ###
    private async Task ReadAllSecretsAsync()
    {
        try
        {
            // Load secrets from file or other source
            var result = await _secretsManager.GetAllSecretsAsync();

            if (result.Status == OperationStatus.Success)
            {
                var allSecrets = result.Value;
                var secrets = allSecrets.Where(s => s.Platform != StringsConstants.Syncfusion).ToList();

                AllSecrets = new ObservableCollection<SecretItemViewModel>(secrets ?? []);

                foreach (var secretItem in AllSecrets)
                    secretItem.PropertyChanged += SecretItem_PropertyChanged;
            }

        }
        catch (Exception e)
        {
            _logger.LogCritical(e, nameof(ReadAllSecretsAsync));
            System.Windows.Application.Current.Shutdown(1);
        }

    }

    #endregion

    #region ### CREATE NEW SECRET ###
    public async Task AddNewSecretAsync()
    {
        try
        {
            var (success, item) = await _totpManager.AddNewSecretAsync();
            if (success && item != null)
            {
                ResetCodeGenerationLabels();
                AllSecrets.Add(item);
                OnPropertyChanged(nameof(AllSecrets));
                UpdateSearchFilter();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_Adding_New_TOTP);
            _messageService.ShowErrorMessage(UI.ex_Adding_New_TOTP + ": " + ex.Message);
        }
    }
    #endregion

    #region ### DELETE SECRET ###

    internal async Task DeleteSecretAsync(SecretItemViewModel item)
    {
        try
        {
            if (await _totpManager.DeleteSecretAsync(item)) // delete from storage file
            {
                AllSecrets.Remove(item); // delete secret from grid's datasource
                OnPropertyChanged(nameof(AllSecrets));
                UpdateSearchFilter();
                ResetCodeGenerationLabels();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_DeletingSecret);
            _messageService.ShowErrorMessage(string.Format(UI.ex_DeletingSecret_0, ex.Message));
        }
    }

    //// working, but no mvvm
    //public void DeleteSecret2(object item) // but here you get a reference to GridRecordContextMenuInfo
    //{
    //    ArgumentNullException.ThrowIfNull(item, nameof(item));

    //    _msgService.ShowMessage(item.ToString() ?? "Null");


    //    // Your deletion logic
    //}

    #endregion

    #region ### UPDATE SECRET ###

    public async Task UpdateSecretAsync(SecretItemViewModel updated)
    {
        if (updated == null || PreviousVersion == null)
            return;

        try
        {
            await _totpManager.UpdateSecretAsync(PreviousVersion, updated);
            PreviousVersion = null;
            UpdateSearchFilter();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_UpdatingSecret);
            _messageService.ShowErrorMessageDialog(string.Format(UI.ex_UpdatingSecret_0, ex.Message));
        }
    }

    private void OnBeginEdit(SecretItemViewModel item)
    {
        PreviousVersion = new SecretItemViewModel(item.Platform, item.Secret);
        item.IsBeingEdited = true;
        OnPropertyChanged(nameof(ShowActionsColumn));
    }

    // TODO: updating does not trigger validation of the secret ! add validation

    private async Task OnEndEdit(SecretItemViewModel item)
    {
        item.IsBeingEdited = false;
        OnPropertyChanged(nameof(ShowActionsColumn));

        if (PreviousVersion != null && !item.Equals(PreviousVersion))
        {
            var (isValid, error) = SecretsManager.IsValidSecretItem(item);

            if (!isValid)
            {
                _messageService.ShowInfoMessage(error!);
                return;
            }
            else
            {
                await UpdateSecretAsync(item);
            }

        }
        PreviousVersion = null;
    }

    #endregion

    #region ### Row/Field Selection Logic  ###

    private bool _isDoubleClick;


    public async Task OnSelectionChangedAsync()
    {
        var currentKey = SelectedSecret.Platform;
        await Task.Delay(300);
        try
        {
            if (currentKey == SelectedSecret.Platform && !_isDoubleClick)
            {
                await OnSecretSelectedAsync();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, UI.ex_Selecting_Secret);
            _messageService.ShowErrorMessage(UI.ex_Selecting_Secret + ": " + e.Message);
        }
        finally
        {
            _isDoubleClick = false;
        }
    }

    private void OnDoubleClick(SecretItemViewModel item)
    {
        _isDoubleClick = true;
        ResetCodeGenerationLabels();

        foreach (var s in AllSecrets)
            s.IsBeingEdited = false;

        item.IsBeingEdited = !item.IsBeingEdited;
        Debug.WriteLine("OnDoubleClick");
        //OnPropertyChanged(nameof(ShowActionsColumn));
    }

    private void ShowCodeLabels()
    {
        CodeLabelHeight = 40;
        IsCodeCopiedVisible = true;
        IsCodeLabelVisible = true;
        IsQrVisible = true;
    }

    private void ResetCodeGenerationLabels()
    {
        CodeLabelHeight = 0;
        IsCodeCopiedVisible = false;
        IsQrVisible = false;
        IsCodeLabelVisible = false;
        QrCodeImage = null;
        CurrentCodeLabel = string.Empty;
    }

    internal async Task OnSecretSelectedAsync()
    {
        if (SelectedSecret != null && !SelectedSecret.IsBeingEdited && !IsContextmenuOpen)
            try
            {
                await CalculateAndDisplayTotpCode(SelectedSecret);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, UI.ex_Error_Generating_TOTP);
                // This is still here because it's specific to TOTP encoding
                _messageService.ShowErrorMessage(UI.ex_Error_Generating_TOTP + ": " + ex.Message);
            }
    }

    #endregion

    #region ### TOTP Code Generation Logic ###

    private static int _counter;

    public static int Increment()
    {
        return Interlocked.Increment(ref _counter);
    }

    private async Task CalculateAndDisplayTotpCode(SecretItemViewModel secret)
    {
        if (_totpManager.TryComputeCode(secret.Secret, out var totpCode, out var error))
        {
            ResetCodeGenerationLabels();

            // if the user clicks on another row right after the currently selected row, the counter gets incremented
            // as this is an async function and we use an async delay below, we check if the counter is the same, so we know
            // the user didn't click on another row meanwhile 
            // we can therefore make the label lblCopiedCode invisible otherwise we don't
            var localCounter = Increment(); // Increment the counter

            // Update the UI
            CurrentCodeLabel = $"{secret.Platform}: {totpCode}";
            _clipboard.SetText(totpCode!);
            QrCodeImage = _qrService.GenerateQr(secret.Platform, secret.Secret, secret.Account);
            Debug.WriteLine($"showing code labels for {secret.Platform}");
            ShowCodeLabels();

            await _delayService.Delay(2000);

            if (IsCodeCopiedVisible && localCounter == _counter)
            {
                IsCodeCopiedVisible = false;
                Debug.WriteLine("########## Label code hidden  #################");
            }
            else
            {
                Debug.WriteLine("########## Label code hidden  SKIPPEEDD  #################");
            }
        }
        else
        {
            _messageService.ShowErrorMessage(string.Format(UI.ex_Error_Generating_TOTP_0_0, secret.Platform, error));
            await Task.FromResult(error);
        }
    }

    #endregion

    #region ### Search Logic ###

    private void ExecuteSearch()
    {
        try
        {
            UpdateSearchFilter();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_Filtering_Secrets);
            _messageService.ShowErrorMessage(UI.ex_Filtering_Secrets + ": " + ex.Message);
        }
    }

    internal void UpdateSearchFilter()
    {
        FilteredSecrets.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? AllSecrets
            : AllSecrets.Where(x =>
                x.Platform?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false);
        foreach (var item in filtered)
            FilteredSecrets.Add(item);
    }

    public string DeleteLabel => TOTP.Resources.UI.ui_btnDelete;

    #endregion
}
