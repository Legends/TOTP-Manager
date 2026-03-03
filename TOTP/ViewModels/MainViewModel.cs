#region ### USINGS ###
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OtpNet;
using Syncfusion.Linq;
using System;
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
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TOTP.Commands;
using TOTP.Core.Interfaces;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Infrastructure.Common;
using TOTP.Infrastructure.Extensions;
using TOTP.Infrastructure.Parser;
using TOTP.Infrastructure.Services;
using TOTP.Resources;
using TOTP.Security.Interfaces;
using TOTP.Services;
using TOTP.Services.Interfaces;
using TOTP.Validation;
using TOTP.ViewModels.Interfaces;
using TOTP.Views.Interfaces;
using static TOTP.ViewModels.SettingsViewModel;
using Application = System.Windows.Application;
using ValidationError = TOTP.Core.Enums.ValidationError;

#endregion

namespace TOTP.ViewModels;

public partial class MainViewModel : IMainViewModel
{
    #region ### COMMON PROPS AND VARS ###

    private readonly Func<IQrScannerDialogService> _qrScannerDialogFactory;
    private ISettingsService _settingsService;
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

    private SettingsViewModel _SettingsVm;

    public SettingsViewModel SettingsVm
    {
        get => _SettingsVm;
        set
        {
            _SettingsVm = value;
            OnPropertyChanged();// wont initialize properly without this, because Settings is null at the beginning and gets initialized async later,
                                // so the setter is not called on app start and OnPropertyChanged is not triggered
        }
    }

    #endregion

    #region ### SECURITY Fields & Props

    public UnlockViewModel UnlockViewModel { get; }
    public IMainViewSessionController SessionController => _mainViewSessionController;

    private AppSessionLockState _sessionState = AppSessionLockState.Locked;
    public AppSessionLockState SessionState
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

    /// <summary>
    /// Used by MainView and
    /// UnlockView ContentControl for visibility
    /// </summary>
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

    private OtpViewModel _editingSecret;

    /// <summary>
    /// Can contain a new secret (in add mode) or a copy of the selected secret (in edit mode)
    /// </summary>
    public OtpViewModel? CurrentSecretBeingEditedOrAdded
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

    private OtpViewModel _selectedAccount = null!;

    public OtpViewModel? SelectedAccount
    {
        get => _selectedAccount;
        set
        {

            if (_selectedAccount == null || _selectedAccount.ID != value?.ID)
            {
                //IsInlineEditing = false;

                foreach (var item in AllOtps)
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


    public OtpViewModel? PreviousVersion { get; set; }

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

    private ObservableCollection<OtpViewModel> _allOtps;
    public ObservableCollection<OtpViewModel> AllOtps
    {
        get => _allOtps;
        private set
        {
            if (ReferenceEquals(_allOtps, value)) return;
            _allOtps = value;
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
    private readonly IAccountsWorkflowService _accountsWorkflow;
    private readonly IDebounceService _debounceService;
    private readonly IQrCodeService _qrService;
    private readonly IFileDialogService _fileDialogService;
    private readonly IMainViewSessionController _mainViewSessionController;

    private bool _otpLoadedFromStore;
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
        IAccountsWorkflowService accountsWorkflow,
        IDebounceService debounceService,
        IDelayService delayService,
        IFileDialogService fileDialogService,
        IAuthorizationService authorization,
        IMainViewSessionController sessionController,
        UnlockViewModel unlockVm,
        Func<IQrScannerDialogService> qrScannerDialogFactory,
        SettingsViewModelFactory settingsFactory, ISettingsService settingsService)
    {
        IsBusy = true;

        _settingsService = settingsService;
        _settingsFactory = settingsFactory;
        _qrScannerDialogFactory = qrScannerDialogFactory;
        _fileDialogService = fileDialogService;
        _logger = logger;
        _qrService = svcQr;
        _messageService = messageService;
        _debounceService = debounceService;
        _clipboardService = clipboardService;
        _accountsWorkflow = accountsWorkflow;
        _mainViewSessionController = sessionController;

        AllOtps = new ObservableCollection<OtpViewModel>();
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

            var loadResult = await _settingsService.LoadAsync();
            if (loadResult.IsFailed)
            {
                throw new InvalidOperationException(string.Join("; ", loadResult.Errors.Select(e => e.Message)));
            }

            SettingsVm = _settingsFactory(
                CloseSettingsViewCommand,
                SaveSettingsView,
                ExportOtps);

            await SettingsVm.LoadAsync();

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
    public AsyncCommand<OtpViewModel> RowSelectionChangedCommand { get; private set; } = null!;

    #endregion REGION COMMANDS

}

