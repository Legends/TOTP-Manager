#region ### USINGS ###
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OtpNet;
using Syncfusion.Linq;
using Syncfusion.PMML;
using Syncfusion.SfSkinManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TOTP.Commands;
using TOTP.Core.Common;
using TOTP.Core.Enums;
using TOTP.Core.Interfaces;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Helper;
using TOTP.Infrastructure.Adapters;
using TOTP.Infrastructure.Extensions;
using TOTP.Infrastructure.Parser;
using TOTP.Infrastructure.Services;
using TOTP.Resources;
using TOTP.Security.Interfaces;
using TOTP.Security.Models;
using TOTP.Services;
using TOTP.Services.Interfaces;
using TOTP.Validation;
using TOTP.Views;
using static TOTP.ViewModels.SettingsViewModel;
using Application = System.Windows.Application;
using ValidationError = TOTP.Core.Enums.ValidationError;

#endregion

namespace TOTP.ViewModels;

public class MainViewModel : IMainViewModel
{
    #region ### COMMON PROPS AND VARS ###

    private readonly Func<IQrScannerDialogService> _qrScannerDialogFactory;

    public IGridFilterRefresher GridFilterRefresher { get; set; }

    #region SETTINGS

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy == value) return;

            _isBusy = value;
            OnPropertyChanged();
        }
    }

    private bool _isSettingsViewOpen;
    public bool IsSettingsViewOpen
    {
        get => _isSettingsViewOpen;
        set
        {
            if (_isSettingsViewOpen == value) return;

            _isSettingsViewOpen = value;
            OnPropertyChanged();

            if (CloseSettingsViewCommand is RelayCommand closeCmd)
                closeCmd.RaiseCanExecuteChanged();
        }
    }

    private SettingsViewModel _Settings;

    public SettingsViewModel Settings
    {
        get => _Settings;
        set
        {
            _Settings = value;
            OnPropertyChanged();// wont initialize properly without this, because Settings is null at the beginning and gets initialized async later,
                                // so the setter is not called on app start and OnPropertyChanged is not triggered
        }
    }

    #endregion

    #region ### SECURITY Fields & Props

    public UnlockViewModel UnlockViewModel { get; }
    public IMainViewSessionController SessionController => _mainViewSessionController;

    private AppSessionState _sessionState = AppSessionState.Locked;
    public AppSessionState SessionState
    {
        get => _sessionState;
        private set
        {
            if (_sessionState == value)
                return;

            _sessionState = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsUnlocked));
        }
    }

    public bool IsUnlocked => _mainViewSessionController.IsUnlocked;

    #endregion


    private CultureDisplay _selectedCulture;
    private readonly ILogger<MainViewModel> _logger;

    // Totp generation timer
    private Timer? _totpUiTimer;
    public Timer? TotpUiTimer
    {
        get => _totpUiTimer;
        set
        {
            _totpUiTimer = value;
        }
    }
    private Totp? _activeTotp;
    private long _activeStep = -1;


    #region ### FLYOUT PANEL ###

    private bool _isSecretVisible;
    public bool IsSecretVisible
    {
        get => _isSecretVisible;
        set
        {
            if (_isSecretVisible == value) return;
            _isSecretVisible = value;
            OnPropertyChanged();
        }
    }

    public bool IsAddMode
    {
        get => _isAddMode;
        private set
        {
            _isAddMode = value;
            //IsEditPlatformFocused = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EditPanelTitle));
            OnPropertyChanged(nameof(SaveButtonLabel));
            OnPropertyChanged(nameof(CancelButtonLabel));
        }
    }
    private bool _isAddMode;

    public string EditPanelTitle => IsAddMode ? UI.tooltip_AddNew : UI.ui_Edit_Entry;
    public string SaveButtonLabel => IsAddMode ? UI.ui_btnAdd : UI.ui_btnSave;
    public string CancelButtonLabel => UI.ui_btnCancel;

    private bool _isEditPlatformFocused;
    public bool IsEditPlatformFocused
    {
        get => _isEditPlatformFocused;
        set { _isEditPlatformFocused = value; OnPropertyChanged(); }
    }

    // Flyout state + editable copy
    private bool _isEditFlyoutOpen;
    public bool IsEditAddFlyoutOpen
    {
        get => _isEditFlyoutOpen;
        set
        {
            _isEditFlyoutOpen = value;
            IsEditPlatformFocused = value;
            OnPropertyChanged();
        }
    }

    private AccountViewModel _editingSecret;

    /// <summary>
    /// Can contain a new secret (in add mode) or a copy of the selected secret (in edit mode)
    /// </summary>
    public AccountViewModel? CurrentSecretBeingEditedOrAdded
    {
        get => _editingSecret;
        set { _editingSecret = value; OnPropertyChanged(); }
    }

    #endregion

    private bool _showCopySymbol;
    public bool ShowCopySymbol
    {
        get => _showCopySymbol;
        set
        {
            _showCopySymbol = value;
            OnPropertyChanged();

            if (value)
            {
                // Reset after animation finishes
                Task.Delay(1100).ContinueWith(_ =>
                {
                    ShowCopySymbol = false;
                });
            }
        }
    }


    bool _showGenerateQrCodeLink;
    public bool ShowGenerateQrCodeLink
    {
        get => _showGenerateQrCodeLink;
        set
        {
            _showGenerateQrCodeLink = value;
            OnPropertyChanged();
        }
    }

    bool _isProgressPieChartVisible;
    public bool IsProgressPieChartVisible
    {
        get => _isProgressPieChartVisible;
        set
        {
            _isProgressPieChartVisible = value;
            OnPropertyChanged();
        }
    }

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

    bool _isGridInEditMode;
    public bool IsGridEditing
    {
        get => _isGridInEditMode;
        set
        {
            if (_isGridInEditMode == value) return;

            _isGridInEditMode = value;
            OnPropertyChanged();
            OpenFlyoutAddModeCommand.RaiseCanExecuteChanged();
            ToggleSearchBoxCommand.RaiseCanExecuteChanged();
            ScanQrAndAddCommand.RaiseCanExecuteChanged();
        }
    }

    public bool IsContextmenuOpen { get; set; }

    private AccountViewModel _selectedAccount = null!;

    public AccountViewModel? SelectedAccount
    {
        get => _selectedAccount;
        set
        {

            if (_selectedAccount == null || _selectedAccount.ID != value?.ID)
            {
                //IsInlineEditing = false;

                foreach (var item in AllAccounts)
                    item.IsBeingEdited = false;

                _selectedAccount = value;
                OnPropertyChanged();
                //IsProgressPieChartVisible = false;
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


    public AccountViewModel? PreviousVersion { get; set; }

    private bool _isSearchVisible;

    public bool IsSearchVisible
    {
        get => _isSearchVisible;
        set
        {
            _isSearchVisible = value;
            OnPropertyChanged();

            ClearSearchCommand.RaiseCanExecuteChanged();
        }
    }

    private bool _isCodeCopiedLabelVisible;

    public bool IsCodeCopiedLabelVisible
    {
        get => _isCodeCopiedLabelVisible;
        set
        {
            _isCodeCopiedLabelVisible = value;
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
            if (!string.Equals(_searchText, value, StringComparison.OrdinalIgnoreCase))
            {
                _searchText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSearchTextNotEmpty));
                _debounceService.Debounce("Search", 300, ExecuteSearch);
            }

        }
    }

    public bool IsSearchTextNotEmpty => !string.IsNullOrEmpty(_searchText);

    #endregion REGION PROPERTIES AND VARS

    #region ### ObservableCollections ###

    private ObservableCollection<AccountViewModel> _allAccounts;
    public ObservableCollection<AccountViewModel> AllAccounts
    {
        get => _allAccounts;
        private set
        {
            if (ReferenceEquals(_allAccounts, value)) return;
            _allAccounts = value;
            //RebuildSecretsView();
            OnPropertyChanged();
        }
    }

    public Action? RequestGridFilterRefresh { get; set; }


    public ObservableCollection<CultureDisplay> SupportedCultures { get; set; }

    #endregion ObservableCollections

    #region ### SERVICES DECLARATIONS ###

    private readonly IClipboardService _clipboardService;
    private readonly IMessageService _messageService;
    private readonly IOtpManager _otpManager;

    private readonly IDebounceService _debounceService;

    //private readonly DispatcherTimer _debounceTimer;
    private readonly IQrCodeService _qrService;
    private readonly IDelayService _delayService;
    private readonly IFileDialogService _fileDialogService;
    private readonly IAuthorizationService _authorization;
    private readonly IMainViewSessionController _mainViewSessionController;
    private readonly IGlobalProfileStore _globalProfileStore;

    private bool _accountsLoaded;
    private bool _collectionHooked;
    //private string _pendingSearchText;

    #endregion REGION SERVICES

    #region ### Constructor ###

    public MainViewModel(

        ILogger<MainViewModel> logger,
        IQrCodeService svcQr,
        IMessageService messageService,
        IClipboardService clipboardService,
        IConfiguration config,
        IOtpManager otpManager,
        IDebounceService debounceService,
        IDelayService delayService,
        IFileDialogService fileDialogService,
        IAuthorizationService authorization,
        IMainViewSessionController sessionController,
        UnlockViewModel unlockVm,
        Func<IQrScannerDialogService> qrScannerDialogFactory,
        SettingsViewModelFactory settingsFactory,IGlobalProfileStore store)
    {
        IsBusy = true;
        
        _settingsFactory = settingsFactory;
        _qrScannerDialogFactory = qrScannerDialogFactory;
        _fileDialogService = fileDialogService;
        _logger = logger;
        _qrService = svcQr;
        _delayService = delayService;
        _messageService = messageService;
        _debounceService = debounceService;
        _clipboardService = clipboardService;
        _otpManager = otpManager;
        _authorization = authorization;
        _mainViewSessionController = sessionController;

        var rawProfilePath = config.GetSection(StringsConstants.GlobalSettingsProfileStorageFilePath).Value;
        var resolvedProfilePath = Environment.ExpandEnvironmentVariables(rawProfilePath ?? string.Empty);
        _globalProfileStore = new FileGlobalProfileStore(resolvedProfilePath);

        AllAccounts = new ObservableCollection<AccountViewModel>();
        //RebuildSecretsView();
        UnlockViewModel = unlockVm;

        _mainViewSessionController.SessionStateChanged += SessionController_SessionStateChanged;
        _mainViewSessionController.ConfigureCallbacks(OnUnlockedAsync, OnLocked);

        SetupCommandEventhandler();

        SetupLocalization();

        //Setup TOTP Code generation timer
        TotpUiTimer = new System.Threading.Timer(_ => StartTotpTick(), null, Timeout.Infinite, 500);


    }

    #endregion

    #region ###  ENTRY-POINT  ###

    /// <summary>
    /// Initializes the main view and its associated settings asynchronously, preparing the application for user
    /// interaction.
    /// </summary>
    /// <remarks>This method sets up the main view, attaches window commands, loads settings, and initializes
    /// authorization. If initialization fails, a critical error is logged and the user is notified with an error dialog
    /// before the application exits. The method should be called during application startup to ensure the main view and
    /// authorization state are properly configured.</remarks>
    /// <param name="mainWindow">The main window instance to attach commands and initialize the view. Can be null if no window is available.</param>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public async Task InitializeMainViewAsync(IMainWindow? mainWindow)
    {
        try
        {

            //Settings = await SettingsViewModel.CreateAsync(
            //    globalProfileStore: _globalProfileStore,
            //    authorizationService: _authorization,
            //    closeCommand: CloseSettingsViewCommand,
            //    saveAction: SaveSettingsView,
            //    exportTest: TestExport, loggingService: TODO);

            Settings = _settingsFactory(
                CloseSettingsViewCommand,
                SaveSettingsView,
                ExportAccounts);

            await Settings.LoadAsync();

            await _mainViewSessionController.InitializeAsync(mainWindow);

            IsBusy = false;

        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "MainViewModel initialization failed.");
            _messageService.ShowError(UI.ex_FatalError + ": " + ex.Message);
            Environment.Exit(1);
        }
    }

    #endregion

    #region ### COMMANDS DECLARATION ###

    public ICommand OpenSettingsCommand { get; private set; } = null!;

    private ICommand _closeSettingsViewCommand;

    public ICommand CloseSettingsViewCommand
    {
        get => _closeSettingsViewCommand;
        private set
        {
            _closeSettingsViewCommand = value;
            OnPropertyChanged();
        }
    }

    public ICommand CopyCodeCommand { get; private set; } = null!;
    public ICommand GenerateQrCommand { get; private set; } = null!;
    public AsyncCommand ExportSecretsCommand { get; private set; } = null!;
    public AsyncCommand ScanQrAndAddCommand { get; private set; } = null!;
    public ICommand CancelFlyoutCommand { get; private set; } = null!;
    public AsyncCommand SaveEditFlyoutAsyncCommand { get; private set; } = null!;
    public ICommand OpenFlyoutEditModeCommand { get; private set; } = null!;
    public RelayCommand OpenFlyoutAddModeCommand { get; private set; } = null!;
    public ICommand BeginEditCommand { get; private set; } = null!;
    public ICommand EndEditCommand { get; private set; } = null!;
    public RelayCommand ClearSearchCommand { get; private set; } = null!;
    public ICommand DeleteSecretCommand { get; private set; } = null!;
    public ICommand DoubleClickCommand { get; private set; } = null!;
    public RelayCommand ToggleSearchBoxCommand { get; private set; } = null!;
    public ICommand UpdateSecretCommand { get; private set; } = null!;
    public AsyncCommand<AccountViewModel> RowSelectionChangedCommand { get; private set; } = null!;

    #endregion REGION COMMANDS

    #region ### COMMANDS EVENTHANDLER ###

    private void SetupCommandEventhandler()
    {
        CloseSettingsViewCommand = new RelayCommand(
             CloseSettingsView,
             () => IsSettingsViewOpen);

        OpenSettingsCommand = new RelayCommand(OpenSettingsView);

        CopyCodeCommand = new RelayCommand<AccountViewModel>(model => CopyTotpCodeToClipboard());
        GenerateQrCommand = new RelayCommand<AccountViewModel>(model => GenerateQrCodeImage());
        ExportSecretsCommand = new AsyncCommand(ExportSecretsToFile);
        ScanQrAndAddCommand = new AsyncCommand(ScanQrAndAddAccountAsync, () => !_isGridInEditMode);

        OpenFlyoutEditModeCommand = new RelayCommand<AccountViewModel>(OpenFlyoutEditMode);
        OpenFlyoutAddModeCommand = new RelayCommand(OpenFlyoutAddMode, () => !_isGridInEditMode);
        SaveEditFlyoutAsyncCommand = new AsyncCommand(AddOrUpdateAccountAsync);
        CancelFlyoutCommand = new RelayCommand(CancelFlyout);

        RowSelectionChangedCommand = new AsyncCommand<AccountViewModel>(OnRowSelectionChangedAsync);
        DeleteSecretCommand = new AsyncCommand<AccountViewModel>(DeleteAccountAsync, null, _logger);
        BeginEditCommand = new RelayCommand<AccountViewModel>(OnBeginEdit);
        EndEditCommand = new AsyncCommand<AccountViewModel>(OnEndEditAsync);
        DoubleClickCommand = new RelayCommand<AccountViewModel>(OnDoubleClick);

        ToggleSearchBoxCommand = new RelayCommand(() =>
        {
            IsSearchVisible = !IsSearchVisible;
            IsSearchFocused = IsSearchVisible;

            if (!IsSearchVisible)
                SearchText = string.Empty;

        }, () => !IsGridEditing);

        ClearSearchCommand = new RelayCommand(ClearSearchTextbox, () => IsSearchVisible);

    }


    private void OpenSettingsView()
    {
        IsSettingsViewOpen = true;
        Settings.RequestFocus();
    }

    private void CloseSettingsView()
    {
        IsSettingsViewOpen = false;
    }

    #endregion COMMANDS SETUP

    private void SaveSettingsView()
    {
        IsSettingsViewOpen = false;
    }

    private async Task ExportAccounts(bool toBeEncrypted)
    {
        try
        {
            System.Windows.MessageBox.Show(toBeEncrypted.ToString());
            var path = _fileDialogService.ShowSaveFileDialog(".txt|.json", ".json", "Totp-Accounts");

            if (path == null) // canceled
                return;

            var result = await _otpManager.GetAllOtpEntriesSortedAsync();
            if (result.IsFailed)
            {
                _messageService.ShowResultError(result);
                return;
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(result.Value, options));

            var psi = new ProcessStartInfo { FileName = path, UseShellExecute = true };

            Process.Start(psi);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    #region ### LOCALIZATION SETUP ###

    private void SetupLocalization()
    {
        SupportedCultures =
        [
            new(new CultureInfo("en"), StringsConstants.ImgUrl.EnFlag),
            new(new CultureInfo("de-DE"), StringsConstants.ImgUrl.DeFlag),
        ];

        var currentCulture = CultureInfo.CurrentUICulture;

        var selCulture = SupportedCultures.FirstOrDefault(c => c.Culture.Name == currentCulture.Name)
                         ?? SupportedCultures.First();
        SelectedCulture = selCulture;

        LocalizationService.LanguageChanged += LocalizationService_LanguageChanged;
    }

    /// <summary>
    /// Put all localizations updates here if not captured in xaml using:
    /// for example:
    ///  -  ToolTip="{xaml:Resx Key=ui_Options}"
    ///  -  ToolTip="{xaml:Resx Key=ui_btnCancel}"
    /// </summary>
    private void LocalizationService_LanguageChanged()
    {
        OnPropertyChanged(nameof(ExportToolTip));
    }

    #endregion

    // todo: https://chatgpt.com/c/6980aa8e-87f0-8396-88d2-a7334f839168
    // todo: use svg and show a zoomed version on click:
    //  <Image ToolTip="{xaml:Resx Key=tooltip_AuthenticatorAppScan}"



    private async Task EnsureAccountsLoadedAsync()
    {
        if (_accountsLoaded)
            return;

        await ReadAllAccountsAsync();

        // Re-Apply the filter function because it is lost after replacing the AllAccounts prop
        // with a new ObservableCollection in ReadAllAccountsAsync !
        GridFilterRefresher.ApplySearchFilter(((IMainViewModel)this).DoFilterGrid);

        if (!_collectionHooked)
        {
            AllAccounts.CollectionChanged += Source_CollectionChanged;
            _collectionHooked = true;
        }

        _accountsLoaded = true;
    }

    private void Source_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (AccountViewModel item in e.NewItems)
            {
                item.SetDuplicateCheck(DuplicateCheck);

            }
        }
    }

    private ValidationError DuplicateCheck(AccountViewModel si)
    {
        return UiValidation.PlatformNameDuplicateExists(si.Issuer, AllAccounts.Where(item => !item.Equals(si)).Select(it => it.ToDomain()).ToList());
    }


    #region ### AUTHORIZATION ###

    private void SessionController_SessionStateChanged(object? sender, AppSessionState state)
    {
        SessionState = state;
    }

    private async Task OnUnlockedAsync()
    {
        await EnsureAccountsLoadedAsync();
    }

    /// <summary>
    /// We reset the application when it enters the locked state.
    /// This ensures that secrets are not accessible in memory and that the app returns to a clean state.
    /// </summary>
    private void OnLocked()
    {
        _accountsLoaded = false;
        AllAccounts.Clear();

        StopTotpTimer();
        ClearCodeGenerationOutput();
        ClearSearchTextbox();
        CancelFlyout();
        IsSettingsViewOpen = false;
        IsSecretVisible = false;
        IsGridEditing = false;
        IsInlineEditing = false;
        SelectedAccount = null; // todo: check flag, was soll bei einem session lock passieren mit dem katuellen zustand
    }


    #endregion

    #region ### OnPropertyChanged ###
    //public event EventHandler<CultureInfo>? LanguageChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region ### READ ALL ACCOUNTS FROM STORAGE FILE ###

    /// <summary>
    /// Reads all secrets from the storage file and populates the AllSecrets collection
    /// </summary>
    /// <returns></returns>
    private async Task ReadAllAccountsAsync()
    {
        try
        {
            // Load secrets from file or other source
            var result = await _otpManager.GetAllOtpEntriesSortedAsync();

            if (result.IsFailed)
            {
                _messageService.ShowResultError(result);
                return;
            }


            //result.Value.Sort(new Comparison<AccountItem>((a, b) => string.Compare(a.Platform, b.Platform, StringComparison.OrdinalIgnoreCase)));

            var allAccounts = result.Value ?? [];
            AllAccounts = new ObservableCollection<AccountViewModel>((allAccounts.Select(item => item.ToViewModel()) ?? []));

            foreach (var item in AllAccounts)
            {
                item.SetDuplicateCheck(DuplicateCheck);
            }

        }
        catch (Exception e)
        {
            _logger.LogCritical(e, nameof(ReadAllAccountsAsync));
            System.Windows.Application.Current.Shutdown(1);
        }

    }

    #endregion

    #region ### OPEN FLYOUT IN ADD MODE ###
    /// <summary>
    /// Opens the flyout panel to add a new secret
    /// Sets IsAddMode to true and creates a new EditingSecret instance
    /// </summary>
    public void OpenFlyoutAddMode()
    {
        try
        {
            IsAddMode = true;
            IsEditAddFlyoutOpen = true;
            CurrentSecretBeingEditedOrAdded = new AccountViewModel(Guid.NewGuid(), null, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_Adding_New_TOTP);
            _messageService.ShowError(UI.ex_Adding_New_TOTP + ": " + ex.Message);
        }
    }
    #endregion

    #region ### OPEN FLYOUT IN EDIT MODE ###
    /// <summary>
    /// Triggered by contextmenu Edit button
    /// Opens the flyout panel and sets IsAddMode to false
    /// </summary>
    /// <param name="item"></param>
    public void OpenFlyoutEditMode(AccountViewModel item)
    {
        if (item == null) return;

        IsAddMode = false;
        CurrentSecretBeingEditedOrAdded = item.Copy();
        IsEditAddFlyoutOpen = true;
    }

    #endregion

    #region ### CLOSE/CANCEL FLYOUT ###

    void CancelFlyout()
    {
        IsEditAddFlyoutOpen = false;
        IsAddMode = false;
        IsSecretVisible = false;
        CurrentSecretBeingEditedOrAdded = null;
    }
    #endregion

    #region ### DELETE ACCOUNT ###

    /// <summary>
    /// Contextmenu delete command execution
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    internal async Task DeleteAccountAsync(AccountViewModel item)
    {
        try
        {

            var shouldDelete = _messageService.ConfirmWarning(string.Format(UI.msg_ConfirmDeleteSecret, item.Issuer), UI.ui_btnDelete);
            if (!shouldDelete)
                return;

            var result = await _otpManager.DeleteAsync(item.ToDomain());

            if (result.IsFailed)
            {
                _messageService.ShowResultError(result, item.Issuer);
                return;
            }

            AllAccounts.Remove(item); // delete secret from internal list
            OnPropertyChanged(nameof(AllAccounts));
            if (item.ID == SelectedAccount?.ID)
            {
                StopTotpTimer();
                ClearCodeGenerationOutput();
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_DeletingSecret);
            _messageService.ShowError(string.Format(UI.ex_DeletingSecret_0, ex.Message));
        }
    }

    #endregion

    #region ### UPDATE ACCOUNT ###

    /// <summary>
    /// Called when inline editing ends or when save button in flyout panel is clicked
    /// </summary>
    /// <param name="updated"></param>
    /// <returns></returns>
    public async Task UpdateAccountAsync(AccountViewModel updated)
    {
        try
        {
            var result = await _otpManager.UpdateAsync(PreviousVersion?.ToDomain(), updated.ToDomain());

            if (result.IsFailed)
            {
                _messageService.ShowResultError(result, updated.Issuer ?? string.Empty);
                return;
            }

            //todo: not needed as the item is already update by ref
            var itemToBeUpdated = AllAccounts.FirstOrDefault(s => s.ID == updated.ID);

            itemToBeUpdated?.UpdateSelf(updated); // only update when in flyout edit mode
            OnPropertyChanged(nameof(AllAccounts));

            if (updated.ID == SelectedAccount?.ID && !ShowGenerateQrCodeLink) // update the QR code if it is visible already
                UpdateQRCode();

            PreviousVersion = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_UpdatingSecret);
            _messageService.ShowError(string.Format(UI.ex_UpdatingSecret_0, ex.Message));
        }
    }

    private void UpdateQRCode()
    {
        QrCodeImage = GenerateQRCodeImage(SelectedAccount);
    }


    /// <summary>
    /// Triggered by save button in flyout panel
    /// Adding/Updating a SecretItem
    /// </summary>
    /// <returns></returns>
    public async Task AddOrUpdateAccountAsync()
    {
        IsSecretVisible = false;

        if (IsAddMode) // add new mode
        {
            CurrentSecretBeingEditedOrAdded?.SetDuplicateCheck(DuplicateCheck);
            var validation = ValidateAccountItem(CurrentSecretBeingEditedOrAdded);

            if (!validation.IsValid)
            {
                CurrentSecretBeingEditedOrAdded.RefreshValidation();
                return;
            }

            var result = await _otpManager.AddNewAsync(CurrentSecretBeingEditedOrAdded.ToDomain());

            if (result.IsFailed)
            {
                _messageService.ShowResultError(result, CurrentSecretBeingEditedOrAdded.Issuer);
                return;
            }

            var itemToAdd = CurrentSecretBeingEditedOrAdded.Copy();
            AllAccounts.Add(itemToAdd);

            CurrentSecretBeingEditedOrAdded = null;
            IsAddMode = false;
            IsEditAddFlyoutOpen = false;
        }
        else // Edit mode
        {
            var updated = CurrentSecretBeingEditedOrAdded.Copy();

            if (updated == null)
                return;

            #region VALIDATION OF EDITED SECRET
            var validator = UiValidation.Use(updated, AllAccounts).ValidateAll();

            if (!validator.IsValid)
            {
                CurrentSecretBeingEditedOrAdded.RefreshValidation();
                return;
            }

            validator.PlatformNameDuplicateExists(excludeSelf: true);

            if (!validator.IsValid)
            {
                CurrentSecretBeingEditedOrAdded.RefreshValidation();
                return;
            }
            #endregion

            await UpdateAccountAsync(CurrentSecretBeingEditedOrAdded);
            IsEditAddFlyoutOpen = false;
        }
    }

    #region ### INLINE EDITING LOGIC FOR SYNCFUSION DATAGRID ###
    /// <summary>
    /// SfDataGridEditingBehavior: Triggered by SfDataGrid's cell edit begin event
    /// </summary>
    /// <param name="item"></param>
    private void OnBeginEdit(AccountViewModel item)
    {
        PreviousVersion = item.Copy();
        item.IsBeingEdited = true;
        IsInlineEditing = true;
    }

    public bool IsInlineEditing { get; set; }

    /// <summary>
    /// Inline Editing End Event
    /// SfDataGridEditingBehavior: Triggered by SfDataGrid's cell edit end event
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private async Task OnEndEditAsync(AccountViewModel item)
    {
        if (item.ID != PreviousVersion.ID)
            return;

        item.IsBeingEdited = false;

        if (!AccountViewModelValueComparer.Default.Equals(item, PreviousVersion))
        {
            //var (isValid, error) = SecretsDAL.IsValidSecretItem(item.ToDomain());
            var validation = UiValidation.Use(item).ValidateAll();

            if (!validation.IsValid)
            {
                _messageService.ShowInfo(ValidationMessageMapper.ToMessage(validation.Errors.FirstOrDefault()));
                return;
            }

            try
            {
                // Update the secret if valid
                await UpdateAccountAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, UI.ex_UpdatingSecret);
                _messageService.ShowError(UI.ex_UpdatingSecret);
            }

        }

        PreviousVersion = null;
        IsInlineEditing = false;

    }

    /// <summary>
    /// Triggered by SfDataGrid's Row MouseDoubleClick event
    /// </summary>
    /// <param name="item"></param>
    private void OnDoubleClick(AccountViewModel item)
    {
        _isDoubleClick = true;
        Debug.WriteLine("***** _isDoubleClick = true;  ***");
        //_totpUiTimer?.Dispose();

        //ClearCodeGenerationOutput();

        foreach (var s in AllAccounts)
            s.IsBeingEdited = false;

        item.IsBeingEdited = !item.IsBeingEdited;
        Debug.WriteLine("OnDoubleClick");
    }
    #endregion

    #endregion

    #region ### Row/Field Grid Selection  ###

    private bool _isDoubleClick;


    /// <summary>
    /// Triggered in RowClickBehavior.cs under behaviours
    /// </summary>
    /// <param name="selectedSecretItem"></param>
    /// <returns></returns>
    public async Task OnRowSelectionChangedAsync(AccountViewModel selectedSecretItem)
    {
        if (SelectedAccount != null && IsInlineEditing && SelectedAccount.ID != selectedSecretItem.ID)
            IsInlineEditing = false;

        if (IsGridEditing || IsInlineEditing || selectedSecretItem == null)
        {
            Debug.WriteLine("OnRowSelectionChangedAsync - early return");
            return;
        }

        _isDoubleClick = false;
        await Task.Delay(300); // prevent OnRowSelectionChangedAsync from executing if it is a double click!

        if (_isDoubleClick) // on double-click we dont execute any selection logic
        {
            if (SelectedAccount == null)
                TotpUiTimer?.Dispose();

            return;
        }

        if (SelectedAccount?.ID == selectedSecretItem?.ID) // dont execute selection logic if the secret is already selected
            return;

        SelectedAccount = ComputeTotpCode(selectedSecretItem, out _activeTotp); // pre-compute TOTP code for the selected item
        //_clipboard.SetText(SelectedSecret.TotpCode!);
        //_clipboard.SetText(TotpCode!);
        CopyTotpCodeToClipboard();

        var currentKey = SelectedAccount.Issuer;

        try
        {
            if (currentKey == SelectedAccount.Issuer)
            {
                if (SelectedAccount != null && !SelectedAccount.IsBeingEdited && !IsContextmenuOpen)
                    try
                    {
                        OnRowSelectionImplementation();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, UI.ex_Error_Generating_TOTP);
                        // This is still here because it's specific to TOTP encoding
                        _messageService.ShowError(UI.ex_Error_Generating_TOTP + ": " + ex.Message);
                    }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, UI.ex_Selecting_Secret);
            _messageService.ShowError(UI.ex_Selecting_Secret + ": " + e.Message);
        }
        finally
        {
            _isDoubleClick = false;
        }
    }

    #endregion

    #region ### TOTP Code Generation ###


    public AccountViewModel ComputeTotpCode(AccountViewModel item, out Totp totpInstance)
    {
        if (!UiValidation.IsValidBase32Format(item.Secret))
            throw new FormatException($"Secret is invalid Base32 format, supplied to {nameof(ComputeTotpCode)}");

        var encodedSecret = Base32Encoding.ToBytes(item.Secret);
        totpInstance = new Totp(encodedSecret);

        TotpCode = totpInstance.ComputeTotp();
        RemainingSeconds = totpInstance.RemainingSeconds();

        return item;
    }

    private string _TotpCode;
    public string TotpCode
    {
        get => _TotpCode;
        set
        {
            _TotpCode = value;
            OnPropertyChanged();
        }
    }

    public int PeriodSeconds { get; } = 30;

    int _remainingSeconds;
    public int RemainingSeconds
    {
        get => _remainingSeconds;
        set
        {
            _remainingSeconds = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ElapsedSeconds));
        }
    }

    public int ElapsedSeconds => PeriodSeconds - RemainingSeconds;

    AccountViewModel _lastSelected;
    private readonly SettingsViewModelFactory _settingsFactory;

    private void OnRowSelectionImplementation()
    {

        Debug.WriteLine("CalculateAndDisplayTotpCode");
        // Totp pie chart reset
        if (TotpUiTimer != null)
            TotpUiTimer.Dispose();

        ClearCodeGenerationOutput();
        StartTotpTick();

        IsProgressPieChartVisible = true;
        CopyTotpCodeToClipboard();
        ShowCopySymbol = true;

        // working example secret: JBSWY3DPEHPK3PXP
        // Google Authenticator works best with 160-bit secrets (20 bytes), but 10–32 bytes is acceptable.
        //byte[] secretBytes = RandomNumberGenerator.GetBytes(20); // 20x8 = 160 bits is ideal
        //string base32Secret = Base32Encoding.ToString(secretBytes).TrimEnd('=');

        ShowCodeGenerationOutput();
    }

    private void StartTotpTick()
    {
        TotpUiTimer?.Dispose();
        TotpUiTimer = new System.Threading.Timer(_ =>
        {
            if (_activeTotp is null || SelectedAccount is null)
            {
                return;
            }

            Debug.WriteLine("#######  Timer is running  #####");

            if (_activeTotp is null || SelectedAccount is null) throw new NullReferenceException(nameof(_activeTotp));

            const int period = 30;
            long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long step = unix / period;// example: 58944862, 58944863, ... 58944891, 58944892, 58944893, ...

            _activeStep = step;
            var now = DateTime.UtcNow;

            // Coalesce: only update UI if it actually changed (per second)
            //if (remaining == _lastRemaining) return;
            //_lastRemaining = remaining;

            Application.Current?.Dispatcher.BeginInvoke(
               DispatcherPriority.Render,
               new Action(() =>
               {
                   TotpCode = _activeTotp.ComputeTotp();
                   RemainingSeconds = _activeTotp.RemainingSeconds();
               }));

        }, null, dueTime: 0, period: 800); // 20 fps tick, UI updates only once/sec due to coalesce
    }

    private BitmapImage GenerateQRCodeImage(AccountViewModel item)
    {
        var normalizedSecret = OtpauthParser.NormalizeBase32SecretForUri(item.Secret);
        // For testing:
        var uri = _qrService.BuildOtpAuthUri(item.Issuer, normalizedSecret, item.AccountName); // base32Secret
        byte[] pngBytes = _qrService.GenerateQr(uri);

        var bmp = new BitmapImage();
        using (var ms = new MemoryStream(pngBytes))
        {
            bmp.BeginInit();
            bmp.StreamSource = ms;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();
        }

        return bmp;
    }

    private static int _counter;
    public static int Increment()
    {
        return Interlocked.Increment(ref _counter);
    }

    #endregion

    #region ### Grid Filter Logic ###



    private bool FilterSecrets(object obj)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return true; // no filter => return all rows

        return obj is AccountViewModel vm && (vm.Issuer?.IndexOf(SearchText.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
    }

    /// <summary>
    /// Displays the row if the obj is of type SecretItemViewModel and
    /// if the search text is empty return every row
    /// or
    /// the platform property of the current object contains the search text
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    bool IMainViewModel.DoFilterGrid(object obj)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        Debug.WriteLine("---  DoFilterGrid   ----");
        return obj is AccountViewModel vm && (vm.Issuer?.IndexOf(SearchText.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
    }

    /// <summary>
    /// Executes when prop SearchText changes
    /// </summary>
    private void ExecuteSearch()
    {
        try
        {
            GridFilterRefresher.Refresh();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_Filtering_Secrets);
            _messageService.ShowError(UI.ex_Filtering_Secrets + ": " + ex.Message);
        }
    }

    // Used in MainWindow.xaml
    public string DeleteLabel => TOTP.Resources.UI.ui_btnDelete;
    // Used in MainWindow.xaml
    public string EditLabel => TOTP.Resources.UI.ui_btnEdit;
    public string ExportToolTip => Resources.UI.ui_Export; // or your resource accessor

    #endregion

    #region ### QR Code - Create - Scan - Add ###

    private void GenerateQrCodeImage()
    {
        var bmp = GenerateQRCodeImage(SelectedAccount);
        QrCodeImage = bmp;
        ShowGenerateQrCodeLink = false;
        IsQrVisible = true;
    }

    /// <summary>
    ///  Triggered by the "Scan QR" camera button
    /// </summary>
    /// <returns></returns>
    public async Task ScanQrAndAddAccountAsync()
    {
        if (Application.Current.MainWindow == null) // todo: change
            return;

        var decodedQrCode = _qrScannerDialogFactory().ScanQrCode(Application.Current.MainWindow);

        if (!string.IsNullOrWhiteSpace(decodedQrCode))
        {
            OtpauthParser.TOTPData? otp = null;

            try
            {
                otp = OtpauthParser.Parse(decodedQrCode);
            }
            catch (Exception e)
            {
                _logger.LogError(e, null, null);
                _messageService.ShowError(UI.msg_ErrorParsingOtpUrl);
                return;
            }

            var newAccountItem = new AccountViewModel(Guid.NewGuid(), otp.Issuer, otp.SecretBase32, otp.Label);

            #region ### validation ###

            var validator = ValidateAccountItem(newAccountItem);
            if (!validator.IsValid)
            {

                foreach (var error in validator.Errors)
                {
                    if (error == ValidationError.PlatformAlreadyExists)
                    {
                        _messageService.ShowError(ValidationMessageMapper.ToMessage(error, newAccountItem.Issuer));
                    }
                    else
                        _messageService.ShowError(ValidationMessageMapper.ToMessage(error));
                }

                return;
            }

            #endregion

            try
            {
                var result = await _otpManager.AddNewAsync(newAccountItem.ToDomain());
                if (result.IsFailed)
                {
                    _messageService.ShowResultError(result, newAccountItem.Issuer);
                    return;
                }

                AllAccounts.Add(newAccountItem);

            }
            finally
            {
                IsAddMode = false;
                IsEditAddFlyoutOpen = false;
            }
        }
    }

    private UiValidation ValidateAccountItem(AccountViewModel newAccountItem)
    {
        ArgumentNullException.ThrowIfNull(newAccountItem);
        return UiValidation.Use(newAccountItem, AllAccounts).ValidateAll().PlatformNameDuplicateExists();
    }

    #endregion

    #region ### EXPORT SECRETS TO EXTERNAL FILE ###
    private async Task ExportSecretsToFile()
    {
        var path = _fileDialogService.ShowSaveFileDialog(".txt|.json", ".json", "Totp-Accounts");

        if (path == null) // canceled
            return;

        var result = await _otpManager.GetAllOtpEntriesSortedAsync();
        if (result.IsFailed)
        {
            _messageService.ShowResultError(result);
            return;
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(result.Value, options));

        var psi = new ProcessStartInfo { FileName = path, UseShellExecute = true };

        Process.Start(psi);
    }
    #endregion

    void ClearSearchTextbox()
    {
        if (string.IsNullOrWhiteSpace(SearchText)) // when the search box is empty already and the user pressed ESC, we close search
        {
            IsSearchVisible = false;
            //return;
        }

        SearchText = "";
        // the property doesn't change if IsSearchFocused is already true
        // so, setting true => true doesn't raise onpropertyChanged and therefore no focus occurs
        // A common pattern is to first set it to false, then back to true,
        // to force the property changed notification:
        IsSearchFocused = false;
        IsSearchFocused = IsSearchVisible;

    }

    //private void CopyCode()
    //{
    //    //_clipboard.SetText(SelectedSecret.TotpCode!);
    //    _clipboard.SetText(TotpCode!);
    //    ShowCopySymbol = true;

    //}

    public void CopyTotpCodeToClipboard()
    {
        // Copy and clear in 30 seconds
        _clipboardService.CopyAndScheduleClear(TotpCode, TimeSpan.FromSeconds(30));
        ShowCopySymbol = true;
    }

    private void ShowCodeGenerationOutput()
    {
        CodeLabelHeight = 40;
        IsCodeCopiedLabelVisible = true;
        IsCodeLabelVisible = true;
        IsQrVisible = true;
        ShowGenerateQrCodeLink = true;
    }

    private void ClearCodeGenerationOutput()
    {
        CodeLabelHeight = 0;
        IsCodeCopiedLabelVisible = false;
        IsQrVisible = false;
        IsProgressPieChartVisible = false;
        IsCodeLabelVisible = false;
        QrCodeImage = null;
        CurrentCodeLabel = string.Empty;
        ShowGenerateQrCodeLink = false;
    }

    void StopTotpTimer()
    {
        TotpUiTimer?.Dispose();
    }

}
