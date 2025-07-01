using Github2FA.Commands;
using Github2FA.Helper;
using Github2FA.Interfaces;
using Github2FA.Models;
using Microsoft.Extensions.Configuration;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Github2FA.ViewModels;

public class MainViewModel : IMainViewModel, INotifyPropertyChanged
{
    #region Props and Vars

    public ObservableCollection<SecretItem> Secrets { get; set; }

    public ICommand AddNewTotpCommand { get; set; }
    public ICommand DeleteSecretCommand { get; set; }
    public ICommand UpdateSecretCommand { get; set;  }
    public ICommand BeginEditCommand { get; set; }
    public ICommand EndEditCommand { get; set; }
    public ICommand DoubleClickCommand { get; set; }

    private readonly ITotpManager _totpManager;

    public bool ShowActionsColumn => Secrets.Any(s => s.IsBeingEdited);

    private SecretItem? _selectedSecret;
    public SecretItem? SelectedSecret
    {
        get => _selectedSecret;
        set
        {
            if (!EqualityComparer<SecretItem?>.Default.Equals(_selectedSecret, value))
            {
                foreach (var item in Secrets)
                    item.IsBeingEdited = false;

                _selectedSecret = value;
                OnPropertyChanged();
                OnSecretSelected();
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

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public SecretItem PreviousVersion { get; set; }

    #endregion

    public MainViewModel(
        IConfiguration config,
        ITotpManager totpManager)
    {

        _totpManager = totpManager;
        SetupCommands();
        SetupSecretsSourceWithEvents(config);
        OnPropertyChanged(nameof(ShowActionsColumn));
    }

    private void SetupSecretsSourceWithEvents(IConfiguration config)
    {
        var secrets = config?.AsEnumerable()
            .Where(kv => kv.Key != "syncfusion")
            .Where(pair => pair.Value != null)
            .Select(pair => new SecretItem(pair.Key, pair.Value));

        Secrets = new ObservableCollection<SecretItem>(secrets ?? Enumerable.Empty<SecretItem>());

        foreach (var item in Secrets)
            item.PropertyChanged += SecretItem_PropertyChanged;
    }

    private void SetupCommands()
    {
        AddNewTotpCommand = new RelayCommand(AddNewTotp);
        DeleteSecretCommand = new RelayCommand<SecretItem>(DeleteSecret);
        UpdateSecretCommand = new RelayCommand<SecretItem>(UpdateSecret);
        BeginEditCommand = new RelayCommand<SecretItem>(OnBeginEdit);
        EndEditCommand = new RelayCommand<SecretItem>(OnEndEdit);
        DoubleClickCommand = new RelayCommand<SecretItem>(OnDoubleClick);
    }

    private void AddNewTotp()
    {
        var (success, item) = _totpManager.PromptAndAddTotp();
        if (success && item != null)
        {
            Secrets.Add(item);
            OnPropertyChanged(nameof(Secrets));
        }
    }

    public void DeleteSecret(SecretItem item)
    {
        if (item == null)
            return;

        _totpManager.DeleteSecret(item);
        Secrets.Remove(item);
        OnPropertyChanged(nameof(Secrets));
    }

    public void UpdateSecret(SecretItem updated)
    {
        if (updated == null || PreviousVersion == null)
            return;

        _totpManager.UpdateSecret(PreviousVersion, updated);
        PreviousVersion = null;
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

    private void OnDoubleClick(SecretItem item)
    {
        foreach (var s in Secrets)
            s.IsBeingEdited = false;

        item.IsBeingEdited = !item.IsBeingEdited;
        OnPropertyChanged(nameof(ShowActionsColumn));
    }

    private async void OnSecretSelected()
    {
        if (SelectedSecret != null)
        {
            try
            {
                string platform = SelectedSecret.Key;
                string platformSecret = SelectedSecret.Value;

                var totp = new Totp(Base32Encoding.ToBytes(platformSecret));
                string totpCode = totp.ComputeTotp();

                CurrentCodeLabel = $"{platform}: {totpCode}";
                Clipboard.SetText(totpCode);
                IsCodeCopiedVisible = true;

                await Task.Delay(2500);
                IsCodeCopiedVisible = false;
            }
            catch (ArgumentException ex)
            {
                // This is still here because it's specific to TOTP encoding
                MessageBox.Show($"Invalid Base32 string.\n{ex.Message}", "Error");
            }
        }
    }

    private void SecretItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SecretItem.IsBeingEdited))
            OnPropertyChanged(nameof(ShowActionsColumn));
    }
}
