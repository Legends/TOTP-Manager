using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TOTP.Commands;
using TOTP.Helper;
using TOTP.Interfaces;
using TOTP.Models;

namespace TOTP.ViewModels;

public class MainViewModel : IMainViewModel, INotifyPropertyChanged
{

    #region ### ObservableCollections ###
    public ObservableCollection<SecretItem> AllSecrets { get; set; } = null!;
    public ObservableCollection<SecretItem> FilteredSecrets { get; } = [];
    #endregion ObservableCollections

    #region ### COMMANDS ###
    public ICommand AddNewTotpCommand { get; private set; } = null!;
    public ICommand BeginEditCommand { get; private set; } = null!;
    public ICommand ClearSearchCommand { get; private set; } = null!;
    public ICommand DeleteSecretCommand { get; private set; } = null!;
    public ICommand DeleteSecretCommand2 { get; private set; } = null!;
    public ICommand DoubleClickCommand { get; private set; } = null!;
    public ICommand DoubleTapCommand { get; private set; } = null!;
    public ICommand EndEditCommand { get; private set; } = null!;
    public ICommand SingleTapCommand { get; private set; } = null!;
    public ICommand ToggleSearchBoxCommand { get; private set; } = null!;
    public ICommand UpdateSecretCommand { get; private set; } = null!;

    public ICommand SelectionChangedCommand => new AsyncCommand(OnSelectionChangedAsync);


    #endregion REGION COMMANDS

    #region ### SERVICES ###
    private readonly IClipboardService _clipboard;
    private readonly IMessageService _msgService;
    private readonly ITotpManager _totpManager;
    private readonly IDebounceService _debounceService;
    //private readonly DispatcherTimer _debounceTimer;
    private readonly IQrCodeService _qrService;
    private readonly IDelayService _delayService;
    //private string _pendingSearchText;
    #endregion REGION SERVICES

    #region ### PROPERTIES AND VARS ###

    private readonly ILogger<MainViewModel> _logger;

    private double _codeLabelOpacity = 0.0;
    public double CodeLabelOpacity
    {
        get => _codeLabelOpacity;
        set
        {
            if (_codeLabelOpacity != value)
            {
                _codeLabelOpacity = value;
                OnPropertyChanged();
            }
        }
    }

    private int _codeLabelHeight = 0;

    public int CodeLabelHeight
    {
        get { return _codeLabelHeight; }
        set { _codeLabelHeight = value; OnPropertyChanged(); }
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

    private SecretItem _selectedSecret = null!;
    public SecretItem SelectedSecret
    {
        get => _selectedSecret;
        set
        {
            if (!EqualityComparer<SecretItem?>.Default.Equals(_selectedSecret, value))
            {
                foreach (var item in AllSecrets)
                    item.IsBeingEdited = false;

                _selectedSecret = value;
                OnPropertyChanged();
                //OnSecretSelected();
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


    public SecretItem? PreviousVersion { get; set; }

    private bool _isSearchVisible = false;
    public bool IsSearchVisible
    {
        get => _isSearchVisible;
        set
        {
            _isSearchVisible = value; OnPropertyChanged();
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
        get { return _ssQrVisible; }
        set
        {
            _ssQrVisible = value;
            OnPropertyChanged();
        }
    }

    private bool _IsCodeLabelVisible;

    public bool IsCodeLabelVisible
    {
        get { return _IsCodeLabelVisible; }
        set
        {
            _IsCodeLabelVisible = value;
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

    private readonly DebounceDispatcher _debouncer = new();

    #endregion REGION PROPERTIES AND VARS

    #region ### Events ###
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region ### Constructor ###
    public MainViewModel(
        ILogger<MainViewModel> logger,
        IQrCodeService svcQR,
        IMessageService msgService,
        IClipboardService clipboard,
        IConfiguration config,
        ITotpManager totpManager,
        IDebounceService debounceService,
        IDelayService delayService) // NEW
    {
        _logger = logger;
        _qrService = svcQR;
        _delayService = delayService;
        _msgService = msgService;
        _debounceService = debounceService;
        _clipboard = clipboard;
        _totpManager = totpManager;

        SetupCommands();

        InitDataSource(config);
        OnPropertyChanged(nameof(ShowActionsColumn));
        UpdateFilter();

    }

    #endregion

    private void InitDataSource(IConfiguration config)
    {
        var secrets = config?.AsEnumerable()
            .Where(kv => kv.Key != "syncfusion")
            .Where(pair => pair.Value != null)
            .Select(pair => new SecretItem(pair.Key, pair.Value!));

        AllSecrets = new ObservableCollection<SecretItem>(secrets ?? []);

        foreach (var secretItem in AllSecrets)
            secretItem.PropertyChanged += SecretItem_PropertyChanged;
    }

    #region ### COMMANDS SETUP ###
    private void SetupCommands()
    {
        AddNewTotpCommand = new RelayCommand(AddNewTotp);
        DeleteSecretCommand = new RelayCommand<SecretItem>(DeleteSecret);
        DeleteSecretCommand2 = new BaseCommand(DeleteSecret2);
        //SingleTapCommand = new AsyncCommand(OnSingleTap);
        //DoubleTapCommand = new RelayCommand<object>(OnDoubleTap);

        UpdateSecretCommand = new RelayCommand<SecretItem>(UpdateSecret);
        BeginEditCommand = new RelayCommand<SecretItem>(OnBeginEdit);
        EndEditCommand = new RelayCommand<SecretItem>(OnEndEdit);
        DoubleClickCommand = new RelayCommand<SecretItem>(OnDoubleClick);

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

    #region ### OLD CODE ###
    //private async Task OnSingleTap()
    //{
    //    //MessageBox.Show("Single Tap executed.");
    //    //await OnSecretSelected();
    //    Debug.WriteLine("OnSingelTap");
    //}

    //private void OnDoubleTap(object parameter)
    //{
    //    Debug.WriteLine("OnDoubleTap");
    //    isDoubleClick = true;
    //    //var e = parameter as Syncfusion.UI.Xaml.Grid.GridCellDoubleTappedEventArgs;
    //    //MessageBox.Show($"Double Tap executed on row {e?.RowColumnIndex.RowIndex}");
    //}
    #endregion

    private void AddNewTotp()
    {
        try
        {

            var (success, item) = _totpManager.PromptAndAddTotp();
            if (success && item != null)
            {
                ResetCodeGenerationLabels();
                AllSecrets.Add(item);
                OnPropertyChanged(nameof(AllSecrets));
                UpdateFilter();
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding new TOTP!");
            _msgService.ShowMessage("Error adding new TOTP! Check the log file.");
        }
    }

    #region ### Delete Logic ###
    public void DeleteSecret(SecretItem item)
    {
        if (item == null)
            return;

        try
        {
            if (_totpManager.DeleteSecret(item))
            {
                AllSecrets.Remove(item);
                OnPropertyChanged(nameof(AllSecrets));
                UpdateFilter();
                ResetCodeGenerationLabels();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting secret");
            _msgService.ShowMessage($"Error deleting secret: {ex.Message}", "Error");
        }

    }

    // working, but no mvvm
    public void DeleteSecret2(object? item) // but here you get a reference to GridRecordContextMenuInfo
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        _msgService.ShowMessage(item?.ToString() ?? "Null");

        if (item == null)
            return;

        // Your deletion logic
    }
    #endregion

    #region ### Update logic ###

    public void UpdateSecret(SecretItem updated)
    {
        if (updated == null || PreviousVersion == null)
            return;

        try
        {
            _totpManager.UpdateSecret(PreviousVersion, updated);
            PreviousVersion = null;
            UpdateFilter();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating secret");
            _msgService.ShowMessage($"Error updating secret: {ex.Message}", "Error");
        }

    }

    private void OnBeginEdit(SecretItem item)
    {
        PreviousVersion = new SecretItem(item.Platform, item.Secret);
        item.IsBeingEdited = true;
        OnPropertyChanged(nameof(ShowActionsColumn));
    }

    private void OnEndEdit(SecretItem item)
    {
        item.IsBeingEdited = false;
        OnPropertyChanged(nameof(ShowActionsColumn));

        if (PreviousVersion != null && !item.Equals(PreviousVersion))
        {
            UpdateSecret(item);
        }
        PreviousVersion = null;
    }
    #endregion



    #region ### Row/Field Selection Logic  ###

    bool isDoubleClick = false;
    private async Task OnSelectionChangedAsync()
    {
        var currentKey = SelectedSecret.Platform;
        await Task.Delay(300);
        try
        {
            if (currentKey == SelectedSecret.Platform && !isDoubleClick)
            {
                //_msgService.ShowMessage(currentKey);
                _ = OnSecretSelected();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error selecting secret");
            _msgService.ShowMessage($"Error selecting secret: {e.Message}", "Error");
        }
        finally
        {
            isDoubleClick = false;
        }

    }

    private void OnDoubleClick(SecretItem item)
    {
        isDoubleClick = true;
        ResetCodeGenerationLabels();

        foreach (var s in AllSecrets)
            s.IsBeingEdited = false;

        item.IsBeingEdited = !item.IsBeingEdited;
        Debug.WriteLine("OnDoubleClick");
        //OnPropertyChanged(nameof(ShowActionsColumn));
    }

    void ShowCodeLabels()
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

    private async Task OnSecretSelected()
    {
        if (SelectedSecret != null && !SelectedSecret.IsBeingEdited && !IsContextmenuOpen)
        {
            try
            {
                await CalculateAndDisplayTotpCode(SelectedSecret);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating TOTP code");
                // This is still here because it's specific to TOTP encoding
                _msgService.ShowMessage($"{ex.Message}", "Error");
            }
        }
    }
    #endregion


    #region ### TOTP Code Generation Logic ###


    private static int _counter;

    public static int Increment()
    {
        return Interlocked.Increment(ref _counter);
    }

    private async Task CalculateAndDisplayTotpCode(SecretItem secret)
    {
        try
        {

            if (_totpManager.TryComputeCode(secret.Secret, out string? totpCode, out string? error))
            {
                ResetCodeGenerationLabels();
                // if the user clicks on another row right after the currently selected row, the counter gets incremented
                // as this is an async function and we use a async delay below, we check if the counter is the same, so we know
                // the user didnt click on another row meanwhile and
                // we can therefore make the label lblCopiedCode invisible otherwise we dont
                var localCounter = Increment(); // Increment the counter

                // Update the UI
                CurrentCodeLabel = $"{secret.Platform}: {totpCode}";
                _clipboard.SetText(totpCode!);
                QrCodeImage = _qrService.GenerateQr(secret.Platform, secret.Secret, secret.Account);

                ShowCodeLabels();

                await _delayService.Delay(2000);
                if (IsCodeCopiedVisible && localCounter == _counter)
                {
                    IsCodeCopiedVisible = false;
                }
            }
            else
            {
                _msgService.ShowMessage($"Error generating TOTP code for {secret.Platform}: {error}", "Error");
                await Task.FromResult(error);
            }

        }
        catch (Exception)
        {
            throw;
        }
    }


    #endregion

    private void SecretItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SecretItem.IsBeingEdited))
            OnPropertyChanged(nameof(ShowActionsColumn));
    }

    #region ### Search Logic ###
    private void ExecuteSearch()
    {
        try
        {
            UpdateFilter();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering secrets");
            _msgService.ShowMessage($"Error filtering secrets: {ex.Message}", "Error");
        }
    }

    private void UpdateFilter()
    {
        FilteredSecrets.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? AllSecrets
            : AllSecrets.Where(x =>
                (x.Platform?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
        foreach (var item in filtered)
            FilteredSecrets.Add(item);
    }
    #endregion

}
