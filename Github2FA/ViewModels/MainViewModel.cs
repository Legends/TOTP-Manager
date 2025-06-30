using Github2FA.Commands;
using Github2FA.Helper;
using Github2FA.Interfaces;
using Github2FA.Models;
using Github2FA.Services;
using Microsoft.Extensions.Configuration;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Github2FA.ViewModels
{
    public class MainViewModel : IMainViewModel, INotifyPropertyChanged
    {

        #region ### prop and vars ###

        public ObservableCollection<SecretItem> Secrets { get; }

        #region COMMANDS
        public ICommand AddNewTotpCommand { get; }
        public ICommand DeleteSecretCommand { get; }
        public ICommand UpdateSecretCommand { get; }
        public ICommand BeginEditCommand { get; }
        public ICommand EndEditCommand { get; }
        public ICommand DoubleClickCommand { get; }

        #endregion

        private readonly IDialogService _dialogService;
        private readonly IConfiguration _config;
        private readonly IMessageService _messageService;
        private readonly ISecretsHelper _secretsHelper;

        public bool ShowActionsColumn => Secrets.Any(s => s.IsBeingEdited);



        private SecretItem? _selectedSecret;
        public SecretItem? SelectedSecret
        {
            get => _selectedSecret;
            set
            {
                if (!EqualityComparer<SecretItem?>.Default.Equals(_selectedSecret, value))
                {
                    // Reset edit mode on all rows
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

        private SecretItem? _activeToolsRow;
        public SecretItem? ActiveToolsRow
        {
            get => _activeToolsRow;
            set
            {
                _activeToolsRow = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Version before updated secreetItem state
        /// </summary>
        public SecretItem PreviousVersion { get; set; }


        #endregion

        public MainViewModel(IDialogService dialogService,
                            IConfiguration config,
                            IMessageService messageService,
                            ISecretsHelper secretesHelper)
        {

            _secretsHelper = secretesHelper;
            _messageService = messageService;
            _config = config;
            _dialogService = dialogService;
            AddNewTotpCommand = new RelayCommand(addNewTotp);
            DeleteSecretCommand = new RelayCommand<SecretItem>(DeleteSecret);
            BeginEditCommand = new RelayCommand<SecretItem>(OnBeginEdit);
            EndEditCommand = new RelayCommand<SecretItem>(OnEndEdit);
            DoubleClickCommand = new RelayCommand<SecretItem>(OnDoubleClick);

            //UpdateSecretCommand = new RelayCommand<SecretItem>(UpdateSecret);

            var secrets = _config?.AsEnumerable().Where(kv => kv.Key != "syncfusion") // Exclude syncfusion key
                          ?.Where(pair => pair.Value != null) // Filter out null values
                          .Select(pair => new SecretItem(pair.Key, pair.Value));

            Secrets = new(secrets ?? Enumerable.Empty<SecretItem>());


            foreach (var item in Secrets)
                item.PropertyChanged += SecretItem_PropertyChanged;

            Secrets.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    // e.OldItems contains deleted items
                    // Handle additional logic here (like updating secrets.json)
                }
            };

            /// Even though the property is false, 
            /// the behavior doesn’t get a notification unless the property changes.
            /// This ensures that when the behavior is attached, 
            /// it gets the property notification and will call UpdateColumnVisibility().
            OnPropertyChanged(nameof(ShowActionsColumn));

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
                UpdateSecret(PreviousVersion, item);
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


        private void SecretItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SecretItem.IsBeingEdited))
                OnPropertyChanged(nameof(ShowActionsColumn));
        }
        private void addNewTotp()
        {
            string? lastKey = null;
            string? lastValue = null;

            while (true)
            {
                var (success, key, value) = _dialogService.ShowKeyValueDialog(lastKey, lastValue);

                // Cancel button pressed
                if (!success)
                    return;

                // Keep the latest entered values
                lastKey = key;
                lastValue = value;

                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                {
                    _messageService.ShowMessage("Key and Value cannot be empty.", "Error");
                    continue; // Prompt again with same values
                }

                // Validate Base32
                if (!IsValidBase32Format(value))
                {
                    _messageService.ShowMessage("Secret must be a valid Base32 string.", "Error");
                    continue; // Prompt again with same values
                }

                // Try to add the secret
                if (_secretsHelper.AddNewItemToSecretsFile(key, value))
                {
                    Secrets.Add(new SecretItem(key, value));
                    OnPropertyChanged(nameof(Secrets));
                    return; // Success, exit loop
                }
                else
                {
                    _messageService.ShowMessage($"Failed to set secret: {key}", "Error");
                    return; // Exit on failure
                }
            }
        }


        private bool IsValidBase32Format(string value)
        {
            try
            {
                _ = OtpNet.Base32Encoding.ToBytes(value);
                return true;
            }
            catch (ArgumentException ex)
            {             
                return false;
            }
        }

        public void UpdateSecret(SecretItem previous, SecretItem updated)
        {

            if (!previous.Equals(updated))
            {
                var current = Secrets.Where(it => it.Key == previous.Key).FirstOrDefault();

                // todo: update secrets.json               
                // we also get the previous
                // because the user might have update the key, so we need the previous key to look for in secrets.json,
                // in order to find the respective entry for updting
                // we need: previous.key and updated object 

                _secretsHelper.UpdateItemInSecretsFile(previous.Key, updated);

                _messageService.ShowMessage($"Updated secret: {previous.Key}");
            }
        }


        public void DeleteSecret(SecretItem item)
        {
            if (item != null)
            {
                try
                {
                    var shouldDelete = _messageService.ShowMessageDialog($"Are you sure you want to delete the secret: {item.Key}?", "Confirm Delete");
                    if (shouldDelete)
                    {
                        Secrets.Remove(item); // Delete from data source
                        _secretsHelper.DeleteItemFromSecretsFile(item.Key); // delete from persistent secrets.json file
                        OnPropertyChanged("Secrets");
                    }
                }
                catch (Exception ex)
                {
                    _messageService.ShowMessage($"Failed to remove secret: {item.Key}");
                }
                finally
                {

                }
            }
        }

        private async void OnSecretSelected()
        {
            if (SelectedSecret != null)
            {
                try
                {
                    string platform = SelectedSecret.Key;
                    string platformSecret = SelectedSecret.Value;

                    // Base32 uses only the characters: A–Z and 2–7.
                    // (So 1, 0, 8, 9 are not allowed.)
                    var totp = new Totp(Base32Encoding.ToBytes(platformSecret));
                    string totpCode = totp.ComputeTotp();

                    CurrentCodeLabel = $"{platform}: {totpCode}";
                    Clipboard.SetText(totpCode);
                    IsCodeCopiedVisible = true;

                    await Task.Delay(2500);
                    IsCodeCopiedVisible = false;
                }
                catch (ArgumentException aex)
                {
                    _messageService.ShowMessage($"Secret is not a Base32 encoded string! {Environment.NewLine}{aex.Message}");
                }
            }
        }
    }
}
