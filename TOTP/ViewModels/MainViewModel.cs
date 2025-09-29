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
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TOTP.Commands;
using TOTP.Comparer;
using TOTP.Core.Enums;
using TOTP.Core.Events;
using TOTP.Core.Interfaces;
using TOTP.Core.Models;
using TOTP.Core.Validation;
using TOTP.Extensions;
using TOTP.Helper;
using TOTP.Interfaces;
using TOTP.Parser;
using TOTP.Resources;
using TOTP.Services;
using TOTP.Validation;
using TOTP.Windows;
#endregion

namespace TOTP.ViewModels;

public class MainViewModel : IMainViewModel, INotifyPropertyChanged //, ILocalizable
{
    #region ### PROPERTIES AND VARS ###

    private CultureDisplay _selectedCulture;
    private readonly ILogger<MainViewModel> _logger;
    private readonly IPlatformSecretDialogService _platformSecretDialogService;


    #region ### FLYOUT PANEL ###
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
    public SecretItemViewModel? EditingSecret
    {
        get => _editingSecret;
        set { _editingSecret = value; OnPropertyChanged(); }
    }

    #endregion

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

    bool _isGridInEditMode;
    public bool IsGridEditing
    {
        get => _isGridInEditMode;
        set
        {
            if (_isGridInEditMode == value) return;

            _isGridInEditMode = value;
            OnPropertyChanged();
            AddNewSecretCommand.RaiseCanExecuteChanged();
            ToggleSearchBoxCommand.RaiseCanExecuteChanged();
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
            //if (!EqualityComparer<SecretItemViewModel?>.Default.Equals(_selectedSecret, value))
            //{
            foreach (var item in AllSecrets)
                item.IsBeingEdited = false;

            _selectedSecret = value;
            OnPropertyChanged();
            //OnSecretSelectedAsync();
            //}
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

    #region ### ObservableCollections ###

    private ObservableCollection<SecretItemViewModel> _allSecrets = new();
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




    private bool DoFilterGrid(object obj)
    {
        return obj is SecretItemViewModel vm && (string.IsNullOrWhiteSpace(SearchText) || vm.Platform?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    /// <summary>
    /// For bulk changes, wrap in using (_view?.DeferRefresh()) { /* add/remove many items */ } to avoid multiple re-filters.
    /// </summary>
    void RefreshView()
    {
        RequestGridFilterRefresh?.Invoke();
    }

    public ObservableCollection<CultureDisplay> SupportedCultures { get; }

    #endregion ObservableCollections

    #region ### SERVICES DECLARATIONS ###

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

        //_totpManager.OnMessageSend += _totpManager_OnMessageSend;
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
        var (success, key, value, account) = _platformSecretDialogService.ShowForm();
        return new AddNewPromptArgs() { Success = success, Platform = key, Secret = value, Account = account };
    }

    private void showMessage(OperationStatus arg1, SecretItemViewModel item)
    {
        switch (arg1)
        {
            case OperationStatus.Unknown:
                _messageService.ShowErrorMessage(item.Error ?? "An unknow error has occured");
                break;
            case OperationStatus.NotFound:
                _messageService.ShowErrorMessage($"{UI.msg_Platform_Not_Found}: {item.Platform}");
                break;
            case OperationStatus.LoadingFailed:
                _messageService.ShowErrorMessage(UI.msg_Failed_Loading_Secrets);
                break;
            case OperationStatus.DeleteFailed:
                _messageService.ShowErrorMessage($"{UI.msg_Failed_Delete_Secret} : {item.Platform}");
                break;
            case OperationStatus.UpdateFailed:
                _messageService.ShowErrorMessage($"{UI.msg_Failed_Updating_Secret} : {item.Platform}");
                break;
            case OperationStatus.CreateFailed:
                _messageService.ShowErrorMessage(string.Format(UI.msg_FailedAddingSecret, item.Platform ?? ""));
                break;
            case OperationStatus.StorageFailed:
                _messageService.ShowErrorMessage($"{UI.msg_Failed_Storage}: {item.Platform ?? ""}");
                break;
            case OperationStatus.Success:
                //_messageService.ShowInfoMessage($"{UI.msg_SecretUpdated}: {item.Platform}");
                break;
            case OperationStatus.AlreadyExists:
                _messageService.ShowErrorMessage(string.Format(UI.msg_Platform_Exists, item.Platform));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(arg1), arg1, null);
        }
    }

    public async Task InitializeAsync()
    {
        await ReadAllSecretsAsync();
    }


    #endregion

    #region ### COMMANDS EVENTHANDLER ###

    private void SetupCommandEventhandler()
    {

        ScanQrAndAddCommand = new AsyncCommand(ScanQrAndAddAsync);

        OpenEditCommand = new RelayCommand<SecretItemViewModel>(OpenEdit);

        SaveEditAsyncCommand = new AsyncCommand(SaveEditAsync);

        CancelEditCommand = new RelayCommand(() =>
        {
            IsEditOpen = false;
            IsAddMode = false;
            EditingSecret = null;
        });

        AddNewSecretCommand = new AsyncCommand(AddNewSecretAsync, () => !_isGridInEditMode, _logger);
        DeleteSecretCommand = new AsyncCommand<SecretItemViewModel>(DeleteSecretAsync, null, _logger);
        UpdateSecretCommand = new AsyncCommand<SecretItemViewModel>(UpdateSecretAsync, null, _logger);
        BeginEditCommand = new RelayCommand<SecretItemViewModel>(OnBeginEdit);
        EndEditCommand = new AsyncCommand<SecretItemViewModel>(OnEndEditAsync); // Method must be: Task OnEndEditAsync()
        //SelectionChangedCommand = new AsyncCommand(async _ => await OnSelectionChangedAsync());
        SelectionChangedCommand = new AsyncCommand(OnSelectionChangedAsync);

        DoubleClickCommand = new RelayCommand<SecretItemViewModel>(OnDoubleClick);

        ToggleSearchBoxCommand = new RelayCommand(() =>
        {
            IsSearchVisible = !IsSearchVisible;
            IsSearchFocused = IsSearchVisible;
        }, () => !IsGridEditing);

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




    #region ### COMMANDS DECLARATION ###

    public AsyncCommand ScanQrAndAddCommand { get; private set; } = null!;
    public ICommand OpenEditCommand { get; private set; } = null!;
    public AsyncCommand SaveEditAsyncCommand { get; private set; } = null!;
    public ICommand CancelEditCommand { get; private set; } = null!;
    public ICommand ChangeLanguageCommand { get; private set; } = null!;
    public AsyncCommand AddNewSecretCommand { get; private set; } = null!;
    public ICommand BeginEditCommand { get; private set; } = null!;
    public ICommand EndEditCommand { get; private set; } = null!;
    public ICommand ClearSearchCommand { get; private set; } = null!;
    public ICommand DeleteSecretCommand { get; private set; } = null!;
    public ICommand DoubleClickCommand { get; private set; } = null!;
    public RelayCommand ToggleSearchBoxCommand { get; private set; } = null!;
    public ICommand UpdateSecretCommand { get; private set; } = null!;
    public ICommand SelectionChangedCommand { get; private set; } = null!;

    #endregion REGION COMMANDS

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
                var secrets = allSecrets.Where(s => s.Platform != StringsConstants.Syncfusion).Select(item => item.ToViewModel()).ToList();

                AllSecrets = new ObservableCollection<SecretItemViewModel>((IEnumerable<SecretItemViewModel>)(secrets ?? []));

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
            //var (success, item) = await _totpManager.AddNewSecretAsync();

            IsAddMode = true;
            IsEditOpen = true;
            EditingSecret = new SecretItemViewModel(Guid.NewGuid(), null, null, null);
            //if (success && item != null)
            //{
            //    ResetCodeGenerationLabels();
            //    AllSecrets.Add(item.ToViewModel());
            //    OnPropertyChanged(nameof(AllSecrets));
            //    UpdateSearchFilter();
            //}
        }
        catch (Exception ex)
        {
            //_errorHandler.Handle(ex, UI.ex_UnexpectedError);

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
            if (await _totpManager.DeleteSecretAsync(item.ToDomain())) // delete from storage file
            {
                AllSecrets.Remove(item); // delete secret from grid's datasource
                OnPropertyChanged(nameof(AllSecrets));
                //ApplySearchFilter();
                ResetCodeGenerationLabels();
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

    public async Task UpdateSecretAsync(SecretItemViewModel updated)
    {
        //PreviousVersion = SelectedSecret.Copy();

        //if (updated == null || PreviousVersion == null)
        //    return;

        try
        {
            //var domainSecretsList = AllSecrets.Where(sivm => !ReferenceEquals(sivm, updated)).Select(s => s.ToDomain()).ToList();
            var domainSecretsList = AllSecrets.Where(sivm => !sivm.Equals(updated)).Select(s => s.ToDomain()).ToList();


            var success = await _totpManager.UpdateSecretAsync(PreviousVersion.ToDomain(), updated.ToDomain(), domainSecretsList);

            if (!success)
                return;

            var itemToBeUpdated = AllSecrets.Where(s => string.Equals(s.Platform, PreviousVersion.Platform, StringComparison.Ordinal)).FirstOrDefault();

            itemToBeUpdated?.UpdateSelf(updated); // only update when in flyout edit mode
            OnPropertyChanged(nameof(AllSecrets));


            PreviousVersion = null;
            //ApplySearchFilter();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_UpdatingSecret);
            _messageService.ShowErrorMessageDialog(string.Format(UI.ex_UpdatingSecret_0, ex.Message));
        }
    }


    public void OpenEdit(SecretItemViewModel item)
    {
        if (item == null) return;

        IsAddMode = false;
        EditingSecret = item.Copy();
        IsEditOpen = true;
        //SaveEditCommand.RaiseCanExecuteChanged();
    }

    public async Task SaveEditAsync()
    {
        if (IsAddMode) // add new mode
        {

            var validator = new UiValidation(EditingSecret);
            validator.ValidateAll();

            if (!validator.IsValid)
            {
                foreach (var error in validator.Errors)
                    _messageService.ShowErrorMessage(ValidationMessageMapper.ToMessage(error));
                return;
            }


            try
            {
                var addResult = await _secretsManager.AddNewItemAsync(EditingSecret.ToDomain());

                if (addResult.Status != OperationStatus.Success)
                {
                    showMessage(addResult.Status, EditingSecret); return;
                }

                //if (result.Status == OperationStatus.Success)
                //    return (true, secretItem);

                //if (result.Status == OperationStatus.AlreadyExists)
                //{
                //    OnMessageSend?.Invoke(this, OperationStatus.AlreadyExists, promptResult.Platform);
                //    continue;
                //}
                //if (result.Status == OperationStatus.StorageFailed)
                //{
                //    OnMessageSend?.Invoke(this, OperationStatus.StorageFailed, promptResult.Platform);
                //}

                //if (result.Status == OperationStatus.LoadingFailed)
                //{
                //    OnMessageSend?.Invoke(this, OperationStatus.LoadingFailed, null);
                //}

                //OnMessageSend?.Invoke(this, OperationStatus.CreateFailed, promptResult.Platform);

                if (addResult.Status == OperationStatus.Success)
                {
                    var itemToAdd = EditingSecret.Copy();
                    //itemToAdd.IsNewlyAdded = true;

                    AllSecrets.Add(itemToAdd);
                    //OnPropertyChanged(nameof(AllSecrets));
                    //ApplySearchFilter();
                    EditingSecret = null;
                }
            }
            finally
            {
                IsAddMode = false;
                IsEditOpen = false;
            }


        }
        else // Edit mode
        {

            PreviousVersion = SelectedSecret.Copy();
            var updated = EditingSecret.Copy();
            if (updated == null || PreviousVersion == null)
                return;

            var validationResult = SecretValidator.ValidatePlatform(updated.Platform);

            if (validationResult != ValidationError.None)
            {
                _messageService.ShowErrorMessage(ValidationMessageMapper.ToMessage(validationResult));
                return;
            }

            validationResult = SecretValidator.ValidatePlatform(updated.Secret);
            if (validationResult != ValidationError.None)
            {
                _messageService.ShowErrorMessage(ValidationMessageMapper.ToMessage(validationResult));
                return;
            }

            var source = AllSecrets.Where(sivm => !sivm.Equals(updated)).Select(s => s.ToDomain()).ToList();

            var hasPlatformNameChanged = !string.Equals(PreviousVersion.Platform, updated.Platform, StringComparison.OrdinalIgnoreCase);
            var duplicateExists = SecretValidator.PlatformNameDuplicateExists(updated.Platform, source) != ValidationError.None;

            if (hasPlatformNameChanged && duplicateExists)
            {
                _messageService.ShowErrorMessage(string.Format(UI.msg_Platform_Exists, updated.Platform));
                return;
            }


            await UpdateSecretAsync(EditingSecret);
            IsEditOpen = false;
        }
    }

    private void OnBeginEdit(SecretItemViewModel item)
    {
        PreviousVersion = item.Copy();
        item.IsBeingEdited = true;
        OnPropertyChanged(nameof(ShowActionsColumn));
    }


    private async Task OnEndEditAsync(SecretItemViewModel item)
    {
        item.IsBeingEdited = false;

        if (!SecretItemValueComparer.Default.Equals(item, PreviousVersion))
        {
            var (isValid, error) = SecretsManager.IsValidSecretItem(item.ToDomain());

            if (!isValid)
            {
                _messageService.ShowInfoMessage(ValidationMessageMapper.ToMessage(error));
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
    }

    #endregion

    #region ### Row/Field Selection Logic  ###

    private bool _isDoubleClick;
    // Source: DataGrid_SelectionChanged in MainViewModel.xaml.cs
    public async Task OnSelectionChangedAsync()
    {
        _isDoubleClick = false;
        var currentKey = SelectedSecret.Platform;
        await Task.Delay(300); // prevent OnSelectionChangedAsync from executing if it is a double click!

        if (_isDoubleClick || SelectedSecret == null) return;
        try
        {
            if (currentKey == SelectedSecret.Platform)
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

    // Source: OnMouseDoubleClick in SfDataGridEditingBehaviors
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
        if (_totpManager.TryComputeCode(secret.Secret, out var totpCode, out var exc))
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

            // working secret: JBSWY3DPEHPK3PXP

            // Google Authenticator works best with 160-bit secrets (20 bytes), but 10–32 bytes is acceptable.
            byte[] secretBytes = RandomNumberGenerator.GetBytes(20); // 20x8 = 160 bits is ideal
            string base32Secret = Base32Encoding.ToString(secretBytes).TrimEnd('=');
            // Fore testing:
            var uri = _qrService.BuildOtpAuthUri(secret.Platform, base32Secret, secret.Account);

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

            QrCodeImage = bmp;

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
            var error = exc is FormatException || exc is ArgumentException
                 ? UI.ex_InvalidSecret
                 : $"{UI.ex_UnexpectedError}.{Environment.NewLine}{exc.Message}";

            _messageService.ShowErrorMessage(string.Format(UI.ex_Error_Generating_TOTP_0_0, secret.Platform, error));
            await Task.FromResult(exc);
        }
    }

    #endregion

    #region ### Search Logic ###

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



    #endregion

    public async Task ScanQrAndAddAsync()
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
            var validator = new UiValidation(newSecretItem);
            validator.ValidateAll();

            if (!validator.IsValid)
            {
                foreach (var error in validator.Errors)
                    _messageService.ShowErrorMessage(ValidationMessageMapper.ToMessage(error));
                return;
            }

            if (SecretValidator.PlatformNameDuplicateExists(newSecretItem.Platform, AllSecrets.Select(o => o.ToDomain()).ToList()) != ValidationError.None)
            {
                _messageService.ShowErrorMessage(string.Format(UI.msg_Platform_Exists, newSecretItem.Platform));
                return;
            }
            #endregion

            try
            {
                var addResult = await _secretsManager.AddNewItemAsync(newSecretItem.ToDomain());
                if (addResult.Status != OperationStatus.Success)
                {
                    showMessage(addResult.Status, newSecretItem);
                    return;
                }
                if (addResult.Status == OperationStatus.Success)
                {
                    //newSecretItem.IsNewlyAdded = true;
                    AllSecrets.Add(newSecretItem);
                    //OnPropertyChanged(nameof(AllSecrets));
                    //ApplySearchFilter();
                }
            }
            finally
            {
                IsAddMode = false;
                IsEditOpen = false;
            }
        }
    }

    bool IMainViewModel.DoFilterGrid(object obj)
    {
        return DoFilterGrid(obj);
    }
}
