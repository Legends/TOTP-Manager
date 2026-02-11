#region ### USINGS ###
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OtpNet;
using Syncfusion.Linq;
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
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TOTP.Commands;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Validation;
using TOTP.Extensions;
using TOTP.Helper;
using TOTP.Parser;
using TOTP.Resources;
using TOTP.Security.Interfaces;
using TOTP.Services;
using TOTP.Services.Interfaces;
using TOTP.Validation;
using TOTP.Views;
using ValidationError = TOTP.Core.Enums.ValidationError;

#endregion

namespace TOTP.ViewModels;

public class MainViewModel : IMainViewModel
{
    #region ### COMMON PROPS AND VARS ###

    #region SETTINGS

    private bool _isSettingsOpen;
    public bool IsSettingsOpen
    {
        get => _isSettingsOpen;
        set
        {
            if (_isSettingsOpen == value) return;

            _isSettingsOpen = value;
            OnPropertyChanged();

            if (CloseSettingsCommand is RelayCommand closeCmd)
                closeCmd.RaiseCanExecuteChanged();
        }
    }

    public SettingsViewModel Settings { get; }

    #endregion

    #region ### SECURITY Fields & Props

    public UnlockViewModel UnlockViewModel { get; }
    public bool IsUnlocked => _authorization.State.IsUnlocked;

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
    private bool _isEditOpen;
    public bool IsEditOpen
    {
        get => _isEditOpen;
        set
        {
            _isEditOpen = value;
            IsEditPlatformFocused = value;
            OnPropertyChanged();
        }
    }

    private SecretItemViewModel _editingSecret;

    /// <summary>
    /// Can contain a new secret (in add mode) or a copy of the selected secret (in edit mode)
    /// </summary>
    public SecretItemViewModel? CurrentSecretBeingEditedOrAdded
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

    private SecretItemViewModel _selectedSecret = null!;

    public SecretItemViewModel SelectedSecret
    {
        get => _selectedSecret;
        set
        {

            if (_selectedSecret == null || _selectedSecret.ID != value?.ID)
            {
                //IsInlineEditing = false;

                foreach (var item in AllSecrets)
                    item.IsBeingEdited = false;

                _selectedSecret = value;
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


    public SecretItemViewModel? PreviousVersion { get; set; }

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
            //ClearCodeGenerationOutput();
            _searchText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSearchTextNotEmpty));
            _debounceService.Debounce("Search", 300, () => ExecuteSearch());
        }
    }

    public bool IsSearchTextNotEmpty => !string.IsNullOrEmpty(_searchText);

    #endregion REGION PROPERTIES AND VARS

    #region ### ObservableCollections ###

    private ObservableCollection<SecretItemViewModel> _allSecrets;
    public ObservableCollection<SecretItemViewModel> AllSecrets
    {
        get => _allSecrets;
        private set
        {
            if (ReferenceEquals(_allSecrets, value)) return;
            _allSecrets = value;
            OnPropertyChanged();
        }
    }

    public Action? RequestGridFilterRefresh { get; set; }  // <-- the view sets this

    public ObservableCollection<CultureDisplay> SupportedCultures { get; set; }

    #endregion ObservableCollections

    #region ### SERVICES DECLARATIONS ###

    private readonly ISecretsDAL _secretsDal;
    private readonly IClipboardService _clipboard;
    private readonly IMessageService _messageService;
    private readonly ISecretsManager _secretsManager;

    private readonly IDebounceService _debounceService;

    //private readonly DispatcherTimer _debounceTimer;
    private readonly IQrCodeService _qrService;
    private readonly IDelayService _delayService;
    private readonly IFileDialogService _fileDialogService;
    private readonly IAuthorizationService _authorization;
    private readonly IUserActivityService _activityService;

    private bool _secretsLoaded;
    private bool _collectionHooked;

    //private string _pendingSearchText;

    #endregion REGION SERVICES

    #region ### Constructor ###

    public MainViewModel(

        ILogger<MainViewModel> logger,
        IQrCodeService svcQr,
        IMessageService messageService,
        IClipboardService clipboard,
        IConfiguration config,
        ISecretsManager totpManager,
        IDebounceService debounceService,
        IDelayService delayService,
        ISecretsDAL secretsDal,
        IFileDialogService fileDialogService,
        IAuthorizationService authorization,
        IUserActivityService activityService,
        UnlockViewModel unlockVM)
    {
        _fileDialogService = fileDialogService;
        _secretsDal = secretsDal;
        _logger = logger;
        _qrService = svcQr;
        _delayService = delayService;
        _messageService = messageService;
        _debounceService = debounceService;
        _clipboard = clipboard;
        _secretsManager = totpManager;
        _authorization = authorization;
        _activityService = activityService;

        AllSecrets = new ObservableCollection<SecretItemViewModel>();
        UnlockViewModel = unlockVM;

        _secretsManager.ConfirmDeleteRequested += _secretsManager_OnDeletePrompt;
        _authorization.State.Changed += AuthorizationState_Changed;
        _activityService.LockRequested += ActivityService_LockRequested;

        SetupCommandEventhandler();

        SetupLocalization();

        //Setup TOTP generation timer
        TotpUiTimer = new System.Threading.Timer(_ => StartTotpTick(), null, Timeout.Infinite, 500);

        Settings = new SettingsViewModel(
            cmdClose: CloseSettingsCommand,
            save: ApplySettings,          // stub for now
            exportTest: TestExport        // stub for now
        );
    }

    #endregion

    #region ### COMMANDS DECLARATION ###

    public ICommand OpenSettingsCommand { get; private set; } = null!;
    public ICommand CloseSettingsCommand { get; private set; } = null!;
    //public ICommand ClearSearchTextCommand => new RelayCommand(() => { SearchText = string.Empty; });
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
    public AsyncCommand<SecretItemViewModel> RowSelectionChangedCommand { get; private set; } = null!;

    #endregion REGION COMMANDS

    #region ### COMMANDS EVENTHANDLER ###

    private void SetupCommandEventhandler()
    {
        CloseSettingsCommand = new RelayCommand(
             CloseSettingsView,
             () => IsSettingsOpen);

        OpenSettingsCommand = new RelayCommand(OpenSettingsView);

        CopyCodeCommand = new RelayCommand<SecretItemViewModel>(model => CopyCode());
        GenerateQrCommand = new RelayCommand<SecretItemViewModel>(model => GenerateQrCodeImage());
        ExportSecretsCommand = new AsyncCommand(ExportSecretsToFile);
        ScanQrAndAddCommand = new AsyncCommand(ScanQrAndAddAccountAsync, () => !_isGridInEditMode);

        OpenFlyoutEditModeCommand = new RelayCommand<SecretItemViewModel>(OpenFlyoutEditMode);
        OpenFlyoutAddModeCommand = new RelayCommand(OpenFlyoutAddMode, () => !_isGridInEditMode);
        SaveEditFlyoutAsyncCommand = new AsyncCommand(AddOrUpdateAsync);
        CancelFlyoutCommand = new RelayCommand(CancelFlyout);

        RowSelectionChangedCommand = new AsyncCommand<SecretItemViewModel>(OnRowSelectionChangedAsync);
        DeleteSecretCommand = new AsyncCommand<SecretItemViewModel>(DeleteSecretAsync, null, _logger);
        BeginEditCommand = new RelayCommand<SecretItemViewModel>(OnBeginEdit);
        EndEditCommand = new AsyncCommand<SecretItemViewModel>(OnEndEditAsync);
        DoubleClickCommand = new RelayCommand<SecretItemViewModel>(OnDoubleClick);

        ToggleSearchBoxCommand = new RelayCommand(() =>
        {
            IsSearchVisible = !IsSearchVisible;
            IsSearchFocused = IsSearchVisible;
        }, () => !IsGridEditing);

        //ClearSearchCommand = new RelayCommand(ClearSearchTextbox);

        ClearSearchCommand = new RelayCommand(ClearSearchTextbox, () => IsSearchVisible);
    }


    private void OpenSettingsView()
    {
        IsSettingsOpen = true;
    }

    private void CloseSettingsView()
    {
        IsSettingsOpen = false;
    }

    #endregion COMMANDS SETUP

    private void ApplySettings()
    {
        // For now just close; later we persist + enforce auth change policy.
        IsSettingsOpen = false;
    }

    private void TestExport()
    {
        // stub
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


    #region ### USER MESSAGES ###
    private bool _secretsManager_OnDeletePrompt(object? sender, string platform)
    {
        return _messageService.ShowWarningMessageDialog(string.Format(UI.msg_ConfirmDeleteSecret, platform));
    }

    private void ShowMessage(OperationStatus arg1, SecretItemViewModel? item)
    {
        switch (arg1)
        {
            case OperationStatus.Unknown:
                _messageService.ShowErrorMessage(item?.Error ?? "An unknow error has occured");
                break;
            case OperationStatus.NotFound:
                _messageService.ShowErrorMessage($"{UI.msg_Platform_Not_Found}: {item?.Platform}");
                break;
            case OperationStatus.LoadingFailed:
                _messageService.ShowErrorMessage(UI.msg_Failed_Loading_Secrets);
                break;
            case OperationStatus.DeleteFailed:
                _messageService.ShowErrorMessage($"{UI.msg_Failed_Delete_Secret} : {item?.Platform}");
                break;
            case OperationStatus.UpdateFailed:
                _messageService.ShowErrorMessage($"{UI.msg_Failed_Updating_Secret} : {item?.Platform}");
                break;
            case OperationStatus.CreateFailed:
                _messageService.ShowErrorMessage(string.Format(UI.msg_FailedAddingSecret, item?.Platform ?? ""));
                break;
            case OperationStatus.StorageFailed:
                _messageService.ShowErrorMessage($"{UI.msg_Failed_Storage}: {item?.Platform ?? ""}");
                break;
            case OperationStatus.Success:
                //_messageService.ShowInfoMessage($"{UI.msg_SecretUpdated}: {item.Platform}");
                break;
            case OperationStatus.AlreadyExists:
                _messageService.ShowErrorMessage(string.Format(UI.msg_Platform_Exists, item?.Platform));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(arg1), arg1, null);
        }
    }

    #endregion

    #region ###  ENTRY-POINT  ###
    public async Task InitializeAsync()
    {
        // Always start locked. The overlay unlock view is visible when IsUnlocked == false.
        OnPropertyChanged(nameof(IsUnlocked));

        // ToDo: enable this when we want to trigger the gate on startup (requirement: gate triggers on every app start)
        await _authorization.InitializeAsync();

        // This triggers the gate every start:
        // - If configured gate is Hello -> it prompts immediately
        // - If password -> returns RequiresUserInput (stays on auth UI)

        // ToDo: enable this when we want to trigger the gate on startup (requirement: gate triggers on every app start)
        await _authorization.TryUnlockOnStartupAsync();

        // Success path is handled by AuthorizationState_Changed → OnUnlockedAsync()
    }

    #endregion

    private async Task EnsureSecretsLoadedAsync()
    {
        if (_secretsLoaded)
        {
            return;
        }

        await ReadAllSecretsAsync();

        if (!_collectionHooked)
        {
            AllSecrets.CollectionChanged += Source_CollectionChanged;
            _collectionHooked = true;
        }

        _secretsLoaded = true;
    }

    private void Source_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (SecretItemViewModel item in e.NewItems)
            {
                item.SetDuplicateCheck(DuplicateCheck);

            }
        }
    }

    private ValidationError DuplicateCheck(SecretItemViewModel si)
    {
        return SecretValidator.PlatformNameDuplicateExists(si.Platform, AllSecrets.Where(item => !item.Equals(si)).Select(it => it.ToDomain()).ToList());
    }


    #region ### AUTHORIZATION ###

    private async void AuthorizationState_Changed(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(IsUnlocked));

        if (IsUnlocked)
        {
            await OnUnlockedAsync();
        }
        else
        {
            OnLocked();
        }
    }

    private async Task OnUnlockedAsync()
    {
        await EnsureSecretsLoadedAsync();
        StartAutoLock();
    }

    /// <summary>
    /// We reset the application when it enters the locked state.
    /// This ensures that secrets are not accessible in memory and that the app returns to a clean state.
    /// </summary>
    private void OnLocked()
    {
        StopAutoLock();

        _secretsLoaded = false;
        AllSecrets.Clear();

        StopTOTPTimer();
        ClearCodeGenerationOutput();
        ClearSearchTextbox();
        CancelFlyout();
        IsSettingsOpen = false;
        IsSecretVisible = false;
        IsGridEditing = false;
        IsInlineEditing = false;
        SelectedSecret = null; // todo: check flag, was soll bei einem session lock passieren mit dem katuellen zustand
    }
    public void Lock()
    {
        // 1. Tell the authorization layer to lock
        _authorization.Lock();

        // 2. Everything else happens via AuthorizationState_Changed
    }


    private void StartAutoLock()
    {
        _activityService.StartMonitoring();
    }

    private void StopAutoLock()
    {
        _activityService.StopMonitoring();
    }

    private void ActivityService_LockRequested(object? sender, EventArgs e)
    {
        _authorization.Lock();
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



    #region ### READ ALL SECRETS FROM STORAGE FILE ###

    /// <summary>
    /// Reads all secrets from the storage file and populates the AllSecrets collection
    /// </summary>
    /// <returns></returns>
    private async Task<ObservableCollection<SecretItemViewModel>?> ReadAllSecretsAsync()
    {
        try
        {
            // Load secrets from file or other source
            var result = await _secretsDal.GetAllSecretsAsync();

            if (result.Status == OperationStatus.Success)
            {
                result.Value.Sort(new Comparison<SecretItem>((a, b) => string.Compare(a.Platform, b.Platform, StringComparison.OrdinalIgnoreCase)));

                var allSecrets = result.Value;
                AllSecrets = new ObservableCollection<SecretItemViewModel>((allSecrets.Select(item => item.ToViewModel()) ?? []));
#if DEBUG
                //for dev purposes, exclude Syncfusion entry

                var secrets = allSecrets.Where(s => s.Platform != StringsConstants.Syncfusion).Select(item => item.ToViewModel()).ToList();

                AllSecrets = new ObservableCollection<SecretItemViewModel>((IEnumerable<SecretItemViewModel>)(secrets ?? []));
#endif
                foreach (var item in AllSecrets)
                {
                    item.SetDuplicateCheck(DuplicateCheck);
                }

                return AllSecrets;
            }
            else
            {
                ShowMessage(result.Status, new SecretItemViewModel(Guid.Empty, null!, null!));
            }

        }
        catch (Exception e)
        {
            _logger.LogCritical(e, nameof(ReadAllSecretsAsync));
            System.Windows.Application.Current.Shutdown(1);
        }
        return null;
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
            IsEditOpen = true;
            CurrentSecretBeingEditedOrAdded = new SecretItemViewModel(Guid.NewGuid(), null, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_Adding_New_TOTP);
            _messageService.ShowErrorMessage(UI.ex_Adding_New_TOTP + ": " + ex.Message);
        }
    }
    #endregion

    #region ### OPEN FLYOUT IN EDIT MODE ###
    /// <summary>
    /// Triggered by contextmenu Edit button
    /// Opens the flyout panel and sets IsAddMode to false
    /// </summary>
    /// <param name="item"></param>
    public void OpenFlyoutEditMode(SecretItemViewModel item)
    {
        if (item == null) return;

        IsAddMode = false;
        CurrentSecretBeingEditedOrAdded = item.Copy();
        IsEditOpen = true;
    }

    #endregion

    #region ### CLOSE/CANCEL FLYOUT ###

    void CancelFlyout()
    {
        IsEditOpen = false;
        IsAddMode = false;
        IsSecretVisible = false;
        CurrentSecretBeingEditedOrAdded = null;
    }
    #endregion

    #region ### DELETE SECRET ###

    /// <summary>
    /// Contextmenu delete command execution
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    internal async Task DeleteSecretAsync(SecretItemViewModel item)
    {
        try
        {
            if (await _secretsManager.DeleteSecretAsync(item.ToDomain())) // delete from storage file
            {
                AllSecrets.Remove(item); // delete secret from internal list
                OnPropertyChanged(nameof(AllSecrets));
                if (item.ID == SelectedSecret?.ID)
                {
                    StopTOTPTimer();
                    ClearCodeGenerationOutput();
                }

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_DeletingSecret);
            _messageService.ShowErrorMessage(string.Format(UI.ex_DeletingSecret_0, ex.Message));
        }
    }

    #endregion

    #region ### UPDATE SECRET ###

    /// <summary>
    /// Called when inline editing ends or when save button in flyout panel is clicked
    /// </summary>
    /// <param name="updated"></param>
    /// <returns></returns>
    public async Task UpdateSecretAsync(SecretItemViewModel updated)
    {
        try
        {
            var success = await _secretsManager.UpdateSecretAsync(PreviousVersion?.ToDomain(), updated.ToDomain());

            if (!success)
                return;

            //todo: not needed as the item is already update by ref
            var itemToBeUpdated = AllSecrets.FirstOrDefault(s => s.ID == updated.ID);

            itemToBeUpdated?.UpdateSelf(updated); // only update when in flyout edit mode
            OnPropertyChanged(nameof(AllSecrets));

            if (updated.ID == SelectedSecret.ID && !ShowGenerateQrCodeLink) // update the QR code if it is visible already
                UpdateQRCode();

            PreviousVersion = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_UpdatingSecret);
            _messageService.ShowErrorMessageDialog(string.Format(UI.ex_UpdatingSecret_0, ex.Message));
        }
    }

    private void UpdateQRCode()
    {
        QrCodeImage = GenerateQRCodeImage(SelectedSecret);
    }


    /// <summary>
    /// Triggered by save button in flyout panel
    /// Adding/Updating a SecretItem
    /// </summary>
    /// <returns></returns>
    public async Task AddOrUpdateAsync()
    {
        IsSecretVisible = false;

        if (IsAddMode) // add new mode
        {
            CurrentSecretBeingEditedOrAdded?.SetDuplicateCheck(DuplicateCheck);
            var validation = IsValidSecretItem(CurrentSecretBeingEditedOrAdded);
            if (!validation.IsValid)
            {
                CurrentSecretBeingEditedOrAdded.RefreshValidation();
                return;
            }

            var addResult = await _secretsDal.AddNewItemAsync(CurrentSecretBeingEditedOrAdded.ToDomain());

            if (addResult.Status != OperationStatus.Success)
            {
                ShowMessage(addResult.Status, CurrentSecretBeingEditedOrAdded);
                return;
            }

            var itemToAdd = CurrentSecretBeingEditedOrAdded.Copy();
            //itemToAdd.IsNewlyAdded = true;

            AllSecrets.Add(itemToAdd);
            //OnPropertyChanged(nameof(AllSecrets));
            //ApplySearchFilter();
            CurrentSecretBeingEditedOrAdded = null;
            IsAddMode = false;
            IsEditOpen = false;


        }
        else // Edit mode
        {
            //PreviousVersion = SelectedSecret.Copy(); // TODO: you can also edit non selected items via flyout !!! we dont need previousversion here!
            var updated = CurrentSecretBeingEditedOrAdded.Copy();

            //if (updated == null || PreviousVersion == null)
            if (updated == null)
                return;

            #region VALIDATION OF EDITED SECRET
            var validator = new UiValidation(updated);
            validator.ValidateAll();

            if (!validator.IsValid)
            {
                //foreach (var error in validator.Errors)
                //    _messageService.ShowErrorMessage(ValidationMessageMapper.ToMessage(error));

                CurrentSecretBeingEditedOrAdded.RefreshValidation();
                return;


            }

            var source = AllSecrets.Where(sivm => !sivm.Equals(updated));

            validator.PlatformNameDuplicateExists(source);

            if (!validator.IsValid)
            {
                //_messageService.ShowErrorMessage(string.Format(UI.msg_Platform_Exists, updated.Platform));
                CurrentSecretBeingEditedOrAdded.RefreshValidation();
                return;
            }
            #endregion

            await UpdateSecretAsync(CurrentSecretBeingEditedOrAdded);
            IsEditOpen = false;
        }
    }

    #region ### INLINE EDITING LOGIC FOR SYNCFUSION DATAGRID ###
    /// <summary>
    /// SfDataGridEditingBehavior: Triggered by SfDataGrid's cell edit begin event
    /// </summary>
    /// <param name="item"></param>
    private void OnBeginEdit(SecretItemViewModel item)
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
    private async Task OnEndEditAsync(SecretItemViewModel item)
    {
        if (item.ID != PreviousVersion.ID)
            return;

        item.IsBeingEdited = false;

        if (!SecretItemViewModelValueComparer.Default.Equals(item, PreviousVersion))
        {
            //var (isValid, error) = SecretsDAL.IsValidSecretItem(item.ToDomain());
            var validation = new UiValidation(item);
            validation.ValidateAll();

            if (!validation.IsValid)
            {
                _messageService.ShowInfoMessage(ValidationMessageMapper.ToMessage(validation.Errors.FirstOrDefault()));
                return;
            }
            else
            {
                try
                {
                    // Update the secret if valid
                    await UpdateSecretAsync(item);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, UI.ex_UpdatingSecret);
                    _messageService.ShowErrorMessage(UI.ex_UpdatingSecret);
                }
            }
        }

        PreviousVersion = null;
        IsInlineEditing = false;

    }

    /// <summary>
    /// Triggered by SfDataGrid's Row MouseDoubleClick event
    /// </summary>
    /// <param name="item"></param>
    private void OnDoubleClick(SecretItemViewModel item)
    {
        _isDoubleClick = true;
        Debug.WriteLine("***** _isDoubleClick = true;  ***");
        //_totpUiTimer?.Dispose();

        //ClearCodeGenerationOutput();

        foreach (var s in AllSecrets)
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
    public async Task OnRowSelectionChangedAsync(SecretItemViewModel selectedSecretItem)
    {
        if (SelectedSecret != null && IsInlineEditing && SelectedSecret.ID != selectedSecretItem.ID)
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
            if (SelectedSecret == null)
                TotpUiTimer?.Dispose();

            return;
        }

        if (SelectedSecret?.ID == selectedSecretItem?.ID) // dont execute selection logic if the secret is already selected
            return;

        SelectedSecret = ComputeTotpCode(selectedSecretItem, out _activeTotp); // pre-compute TOTP code for the selected item
        //_clipboard.SetText(SelectedSecret.TotpCode!);
        _clipboard.SetText(TotpCode!);

        var currentKey = SelectedSecret.Platform;

        try
        {
            if (currentKey == SelectedSecret.Platform)
            {
                if (SelectedSecret != null && !SelectedSecret.IsBeingEdited && !IsContextmenuOpen)
                    try
                    {
                        OnRowSelectionImplementation();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, UI.ex_Error_Generating_TOTP);
                        // This is still here because it's specific to TOTP encoding
                        _messageService.ShowErrorMessage(UI.ex_Error_Generating_TOTP + ": " + ex.Message);
                    }
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

    #endregion

    #region ### TOTP Code Generation ###

    /// <summary>
    /// StepSize ist die Gültigkeitsdauer eines TOTP-Codes in Sekunden. Standardmäßig beträgt sie 30 Sekunden.
    /// </summary>
    /// <param name="secret"></param>
    /// <param name="code"></param>
    /// <param name="remainingSeconds"></param>
    /// <param name="exc"></param>
    /// <returns></returns>
    public bool TryComputeTotpCode(string secret, out string code, out Totp? totpInstance, out Exception? exc)
    {
        code = null;
        totpInstance = null;

        try
        {
            if (!SecretValidator.IsValidBase32Format(secret))
            {
                exc = new FormatException($"Secret is invalid Base32 format, supplied to {nameof(TryComputeTotpCode)}");
                return false;
            }

            var encodedSecret = Base32Encoding.ToBytes(secret);
            totpInstance = new Totp(encodedSecret);
            code = totpInstance.ComputeTotp();

            exc = null;
            return true;
        }
        catch (Exception ex)
        {
            exc = ex;
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }

    public SecretItemViewModel ComputeTotpCode(SecretItemViewModel item, out Totp totpInstance)
    {

        if (!SecretValidator.IsValidBase32Format(item.Secret))
            throw new FormatException($"Secret is invalid Base32 format, supplied to {nameof(ComputeTotpCode)}");

        var encodedSecret = Base32Encoding.ToBytes(item.Secret);
        totpInstance = new Totp(encodedSecret);
        //item.TotpCode = totpInstance.ComputeTotp();
        TotpCode = totpInstance.ComputeTotp();
        //item.RemainingSeconds =  totpInstance.RemainingSeconds();
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

    SecretItemViewModel _lastSelected;
    private void OnRowSelectionImplementation()
    {


        Debug.WriteLine("CalculateAndDisplayTotpCode");
        // Totp pie chart reset
        if (TotpUiTimer != null)
            TotpUiTimer.Dispose();

        ClearCodeGenerationOutput();
        StartTotpTick();

        IsProgressPieChartVisible = true;
        _clipboard.SetText(TotpCode!);
        ShowCopySymbol = true;

        // working example secret: JBSWY3DPEHPK3PXP
        // Google Authenticator works best with 160-bit secrets (20 bytes), but 10–32 bytes is acceptable.
        //byte[] secretBytes = RandomNumberGenerator.GetBytes(20); // 20x8 = 160 bits is ideal
        //string base32Secret = Base32Encoding.ToString(secretBytes).TrimEnd('=');

        ShowCodeGenerationOutput();
    }

    //private int _lastRemaining = -1;
    Guid _lastItemGuid = Guid.Empty;

    private void StartTotpTick()
    {
        TotpUiTimer?.Dispose();
        TotpUiTimer = new System.Threading.Timer(_ =>
        {
            if (_activeTotp is null || SelectedSecret is null)
            {
                return;
            }

            Debug.WriteLine("#######  Timer is running  #####");

            if (_activeTotp is null || SelectedSecret is null) throw new NullReferenceException(nameof(_activeTotp));

            const int period = 30;
            long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long step = unix / period;// example: 58944862, 58944863, ... 58944891, 58944892, 58944893, ...

            //if (step != _activeStep)
            //{
            //    Debug.WriteLine("##################   step != _activeStep    ##################################");
            _activeStep = step;
            var now = DateTime.UtcNow;

            // Coalesce: only update UI if it actually changed (per second)
            //if (remaining == _lastRemaining) return;
            //_lastRemaining = remaining;

            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Render,
                new Action(() =>
                {
                    //SelectedSecret.TotpCode = _activeTotp.ComputeTotp();
                    TotpCode = _activeTotp.ComputeTotp();
                    //SelectedSecret.RemainingSeconds = _activeTotp.RemainingSeconds();
                    RemainingSeconds = _activeTotp.RemainingSeconds();
                    //if (!IsProgressPieChartVisible)
                    //    IsProgressPieChartVisible = true;
                }));

        }, null, dueTime: 0, period: 800); // 20 fps tick, UI updates only once/sec due to coalesce
    }

    private BitmapImage GenerateQRCodeImage(SecretItemViewModel item)
    {
        var normalizedSecret = OtpauthParser.NormalizeBase32SecretForUri(item.Secret);
        // For testing:
        var uri = _qrService.BuildOtpAuthUri(item.Platform, normalizedSecret, item.Account); // base32Secret

        //var uri = _qrService.BuildOtpAuthUri(secret.Platform, secret.Secret, secret.Account);
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
        Debug.WriteLine("---  DoFilterGrid   ----");
        return obj is SecretItemViewModel vm && (string.IsNullOrWhiteSpace(SearchText) || vm.Platform?.IndexOf(SearchText.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
    }

    /// <summary>
    /// For bulk changes, wrap in using (_view?.DeferRefresh()) { /* add/remove many items */ } to avoid multiple re-filters.
    /// </summary>
    void RefreshView()
    {
        RequestGridFilterRefresh?.Invoke();
    }
    private void ExecuteSearch()
    {
        try
        {
            // when SearchText changes:
            RefreshView();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_Filtering_Secrets);
            _messageService.ShowErrorMessage(UI.ex_Filtering_Secrets + ": " + ex.Message);
        }
    }


    public string DeleteLabel => TOTP.Resources.UI.ui_btnDelete;
    public string EditLabel => TOTP.Resources.UI.ui_btnEdit;


    public string ExportToolTip => Resources.UI.ui_Export; // or your resource accessor




    #endregion

    #region ### QR Code - Create - Scan - Add ###

    private void GenerateQrCodeImage()
    {
        var bmp = GenerateQRCodeImage(SelectedSecret);
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
        var dlg = new QrScannerWindow { Owner = System.Windows.Application.Current.MainWindow };
        if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.DecodedText))
        {

            OtpauthParser.TOTPData? data = null;

            try
            {
                data = OtpauthParser.Parse(dlg.DecodedText);
            }
            catch (Exception e)
            {
                _logger.LogError(e, null, null);
                _messageService.ShowErrorMessage(UI.msg_ErrorParsingOtpUrl);
                return;
            }


            var newSecretItem = new SecretItemViewModel(Guid.NewGuid(), data.Issuer, data.SecretBase32, data.Label);

            #region ### validation ###

            var validator = IsValidSecretItem(newSecretItem);
            if (!validator.IsValid)
            {

                foreach (var error in validator.Errors)
                {
                    if (error == ValidationError.PlatformAlreadyExists)
                    {
                        _messageService.ShowErrorMessage(ValidationMessageMapper.ToMessage(error, newSecretItem.Platform));
                    }
                    else
                        _messageService.ShowErrorMessage(ValidationMessageMapper.ToMessage(error));
                }

                return;
            }

            #endregion

            try
            {
                var addResult = await _secretsDal.AddNewItemAsync(newSecretItem.ToDomain());
                if (addResult.Status != OperationStatus.Success)
                {
                    ShowMessage(addResult.Status, newSecretItem);
                    return;
                }
                if (addResult.Status == OperationStatus.Success)
                {
                    AllSecrets.Add(newSecretItem);
                }
            }
            finally
            {
                IsAddMode = false;
                IsEditOpen = false;
            }
        }
    }

    private UiValidation IsValidSecretItem(SecretItemViewModel newSecretItem)
    {
        ArgumentNullException.ThrowIfNull(newSecretItem, nameof(newSecretItem));

        var validator = new UiValidation(newSecretItem);
        validator.ValidateAll().PlatformNameDuplicateExists(AllSecrets);

        return validator;
    }

    #endregion

    #region ### EXPORT SECRETS TO EXTERNAL FILE ###
    private async Task ExportSecretsToFile()
    {
        var path = _fileDialogService.ShowSaveFileDialog(".txt|.json", ".json", "Totp-Secrets");

        if (path == null) // canceled
            return;

        var secrets = await _secretsDal.GetAllSecretsAsync();
        if (secrets.Status != OperationStatus.Success)
        {
            ShowMessage(secrets.Status, null);
            return;
        }

        secrets.Value.Sort(new Comparison<SecretItem>((a, b) => string.Compare(a.Platform, b.Platform, StringComparison.OrdinalIgnoreCase)));
        var options = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(secrets.Value, options));

        var psi = new ProcessStartInfo { FileName = path, UseShellExecute = true };

        Process.Start(psi);
    }
    #endregion

    void ClearSearchTextbox()
    {
        SearchText = "";
        // the property doesn't change if IsSearchFocused is already true
        // so, setting true => true doesn't raise onpropertyChanged and therefore no focus occurs
        // A common pattern is to first set it to false, then back to true,
        // to force the property changed notification:
        IsSearchFocused = false;
        IsSearchFocused = IsSearchVisible;
    }

    private void CopyCode()
    {
        //_clipboard.SetText(SelectedSecret.TotpCode!);
        _clipboard.SetText(TotpCode!);
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

    void StopTOTPTimer()
    {
        TotpUiTimer?.Dispose();
    }

}
