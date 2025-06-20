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
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public ICommand AddNewTotpCommand { get; }
        public ICommand DeleteSecretCommand { get; }
        public ICommand Cmd2Command { get; }
        public ICommand Cmd3Command { get; }

        private readonly IDialogService _dialogService;
        private readonly IConfiguration _config;
        private readonly IMessageService _messageService;
        private readonly ISecretsHelper _secretsHelper;

        private SecretItem? _selectedSecret;
        public SecretItem? SelectedSecret
        {
            get => _selectedSecret;
            set
            {
                if (!EqualityComparer<SecretItem?>.Default.Equals(_selectedSecret, value))
                {
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

            var secrets = _config?.AsEnumerable().Where(kv => kv.Key != "syncfusion") // Exclude syncfusion key
                          ?.Where(pair => pair.Value != null) // Filter out null values
                          .Select(pair => new SecretItem(pair.Key, pair.Value));

            Secrets = new(secrets ?? Enumerable.Empty<SecretItem>());

            Secrets.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    // e.OldItems contains deleted items
                    // Handle additional logic here (like updating secrets.json)
                }
            };
        }


        void addNewTotp()
        {
            var (success, key, value) = _dialogService.ShowKeyValueDialog();
            if (success)
            {
                if (_secretsHelper.AddNewItemToSecretsFile(key, value))
                {
                    Secrets.Add(new(key, value));
                    OnPropertyChanged("Secrets");

                    _messageService.ShowMessage("Key/Value successfully added!", "Success");
                }
                else
                {
                    // Failed to set the secret
                    _messageService.ShowMessage($"Failed to set secret: {key}");
                }
            }
        }

        public void DeleteSecret(SecretItem item)
        {
            if (item != null)
            {
                try
                {
                   
                    Secrets.Remove(item);
                    _secretsHelper.DeleteItemFromSecretsFile(item.Key);
                    OnPropertyChanged("Secrets");
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
        }
    }
}
