using Github2FA.Commands;
using Github2FA.Helper;
using Github2FA.Interfaces;
using Github2FA.Models;
using Github2FA.Services;
using Microsoft.Extensions.Configuration;
using OtpNet;
using Syncfusion.PMML;
using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Github2FA.ViewModels;

public class MainViewModel : IMainViewModel, INotifyPropertyChanged
{

    #region ### ObservableCollections ###
    public ObservableCollection<SecretItem> AllSecrets { get; set; }
    public ObservableCollection<SecretItem> FilteredSecrets { get; } = new();
    #endregion ObservableCollections

    #region ### COMMANDS ###
    public ICommand AddNewTotpCommand { get; private set; }
    public ICommand DeleteSecretCommand { get; private set; }
    public ICommand DeleteSecretCommand2 { get; private set; }
    public ICommand UpdateSecretCommand { get; private set; }
    public ICommand BeginEditCommand { get; private set; }
    public ICommand EndEditCommand { get; private set; }
    public ICommand DoubleClickCommand { get; private set; }
    public ICommand ToggleSearchBoxCommand { get; private set; }
    public ICommand ClearSearchCommand { get; private set; }
    public ICommand SingleTapCommand { get; private set; }
    public ICommand DoubleTapCommand { get; private set; }
    #endregion REGION COMMANDS

    #region ### SERVICES ###
    private readonly IClipboardService _clipboard;
    private readonly IMessageService _msgService;
    private readonly ITotpManager _totpManager;
    private readonly IDebounceService _debounceService;
    private readonly DispatcherTimer _debounceTimer;
    private readonly IDelayService _delayService;
    //private string _pendingSearchText;
    #endregion REGION SERVICES

    #region ### PROPERTIES AND VARS ###
    public bool ShowActionsColumn => AllSecrets.Any(s => s.IsBeingEdited);

    private SecretItem? _selectedSecret;
    public SecretItem? SelectedSecret
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

    private string _currentCodeLabel;
    public string CurrentCodeLabel
    {
        get => _currentCodeLabel;
        set
        {
            _currentCodeLabel = value;
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

    public SecretItem PreviousVersion { get; set; }

    private bool _isSearchVisible = false;
    public bool IsSearchVisible
    {
        get => _isSearchVisible;
        set
        {
            _isSearchVisible = value; OnPropertyChanged();
        }
    }

    private string _searchText;
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSearchTextNotEmpty));
            _debounceService.Debounce("Search", 300, () => ExecuteSearch());
        }
    }

    public bool IsSearchTextNotEmpty => !string.IsNullOrEmpty(_searchText);

    private readonly DebounceDispatcher _debouncer = new DebounceDispatcher();

    #endregion REGION PROPERTIES AND VARS

    #region ### Events ###
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion


    public MainViewModel(
        IMessageService msgService,
        IClipboardService clipboard,
        IConfiguration config,
        ITotpManager totpManager,
        IDebounceService debounceService,
        IDelayService delayService) // NEW
    {
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

    private void InitDataSource(IConfiguration config)
    {
        var secrets = config?.AsEnumerable()
            .Where(kv => kv.Key != "syncfusion")
            .Where(pair => pair.Value != null)
            .Select(pair => new SecretItem(pair.Key, pair.Value));

        AllSecrets = new ObservableCollection<SecretItem>(secrets ?? Enumerable.Empty<SecretItem>());

        foreach (var secretItem in AllSecrets)
            secretItem.PropertyChanged += SecretItem_PropertyChanged;
    }

    private void SetupCommands()
    {
        AddNewTotpCommand = new RelayCommand(AddNewTotp);
        DeleteSecretCommand = new RelayCommand<SecretItem>(DeleteSecret);
        DeleteSecretCommand2 = new BaseCommand(DeleteSecret2);
        //SingleTapCommand = new RelayCommand(async () => await OnSingleTap());
        SingleTapCommand = new AsyncCommand(OnSingleTap);
        DoubleTapCommand = new RelayCommand<object>(OnDoubleTap);

        UpdateSecretCommand = new RelayCommand<SecretItem>(UpdateSecret);
        BeginEditCommand = new RelayCommand<SecretItem>(OnBeginEdit);
        EndEditCommand = new RelayCommand<SecretItem>(OnEndEdit);
        DoubleClickCommand = new RelayCommand<SecretItem>(OnDoubleClick);
        ToggleSearchBoxCommand = new RelayCommand(() => IsSearchVisible = !IsSearchVisible);
        ClearSearchCommand = new RelayCommand(() => SearchText = "");
    }

    private async Task OnSingleTap()
    {
        //MessageBox.Show("Single Tap executed.");
        await OnSecretSelected();
    }

    private void OnDoubleTap(object parameter)
    {
        //var e = parameter as Syncfusion.UI.Xaml.Grid.GridCellDoubleTappedEventArgs;
        //MessageBox.Show($"Double Tap executed on row {e?.RowColumnIndex.RowIndex}");


    }

    private void OnDoubleClick(SecretItem item)
    {
        foreach (var s in AllSecrets)
            s.IsBeingEdited = false;

        item.IsBeingEdited = !item.IsBeingEdited;
        //OnPropertyChanged(nameof(ShowActionsColumn));
    }


    private void AddNewTotp()
    {

        var (success, item) = _totpManager.PromptAndAddTotp();
        if (success && item != null)
        {
            AllSecrets.Add(item);
            OnPropertyChanged(nameof(AllSecrets));
            UpdateFilter();
        }
    }


    public void DeleteSecret(SecretItem item)
    {
        if (item == null)
            return;

        if (_totpManager.DeleteSecret(item))
        {
            AllSecrets.Remove(item);
            OnPropertyChanged(nameof(AllSecrets));
            UpdateFilter();
        }
    }

    // working, but no mvvm
    public void DeleteSecret2(object item) // but here you get a reference to GridRecordContextMenuInfo
    {
        _msgService.ShowMessage(item?.ToString() ?? "Null");

        if (item == null)
            return;

        // Your deletion logic
    }

    public string ViewModelTypeName => GetType().Name;



    public void UpdateSecret(SecretItem updated)
    {
        if (updated == null || PreviousVersion == null)
            return;

        _totpManager.UpdateSecret(PreviousVersion, updated);
        PreviousVersion = null;
        UpdateFilter();
    }

    private void OnBeginEdit(SecretItem item)
    {
        PreviousVersion = new SecretItem(item.Key, item.Value);
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


    private static int _counter;

    public static int Increment()
    {
        return Interlocked.Increment(ref _counter);
    }






    public bool IsContextmenuOpen { get; set; }
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
                // This is still here because it's specific to TOTP encoding
                _msgService.ShowMessage($"{ex.Message}", "Error");
            }
        }
    }

    private async Task CalculateAndDisplayTotpCode(SecretItem secret)
    {
        string? totpCode = null;
        string? error = null;

        if (_totpManager.TryComputeCode(secret.Value, out totpCode, out error))
        {
            var localCounter = Increment(); // Increment the counter

            // Update the UI
            CurrentCodeLabel = $"{secret.Key}: {totpCode}";
            _clipboard.SetText(totpCode);

            IsCodeCopiedVisible = true;

            await _delayService.Delay(2000);
            if (IsCodeCopiedVisible && localCounter == _counter)
            {
                IsCodeCopiedVisible = false;
            }
        }
        else
        {
            _msgService.ShowMessage($"Error generating TOTP code for {secret.Key}: {error}", "Error");
            await Task.FromResult(error);
        }
    }

    private void SecretItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SecretItem.IsBeingEdited))
            OnPropertyChanged(nameof(ShowActionsColumn));
    }

    private void ExecuteSearch()
    {
        UpdateFilter();
    }

    private void UpdateFilter()
    {
        FilteredSecrets.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? AllSecrets
            : AllSecrets.Where(x =>
                (x.Key?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
        foreach (var item in filtered)
            FilteredSecrets.Add(item);
    }

}
