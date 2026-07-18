using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class AccountListViewModel : INotifyPropertyChanged
{
    private readonly IAccountManager _accountManager;
    private readonly AsyncCommand _loadCommand;
    private IReadOnlyList<AccountListItemViewModel> _allAccounts = [];
    private IReadOnlyList<AccountListItemViewModel> _accounts = [];
    private string _message = string.Empty;
    private string _searchText = string.Empty;
    private bool _isBusy;

    public AccountListViewModel(IAccountManager accountManager)
    {
        _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
        _loadCommand = new AsyncCommand(LoadAsync, () => !_isBusy);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<AccountListItemViewModel> Accounts
    {
        get => _accounts;
        private set => SetField(ref _accounts, value);
    }

    public string Message
    {
        get => _message;
        private set
        {
            if (!SetField(ref _message, value)) return;
            OnPropertyChanged(nameof(HasMessage));
        }
    }

    public bool HasMessage => Message.Length > 0;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (!SetField(ref _searchText, value ?? string.Empty)) return;
            ApplyFilter();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetField(ref _isBusy, value)) return;
            _loadCommand.NotifyCanExecuteChanged();
        }
    }

    public ICommand LoadCommand => _loadCommand;

    public async Task LoadAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        Message = string.Empty;
        try
        {
            var result = await _accountManager.GetAllOtpEntriesSortedAsync();
            if (result.IsFailed)
            {
                _allAccounts = [];
                Accounts = [];
                Message = "Accounts could not be loaded. Your encrypted data was not changed.";
                return;
            }

            _allAccounts = result.Value
                .Select(account => new AccountListItemViewModel(
                    account.ID,
                    account.Issuer,
                    account.AccountName ?? string.Empty))
                .ToArray();
            ApplyFilter();
        }
        catch (Exception)
        {
            _allAccounts = [];
            Accounts = [];
            Message = "Accounts could not be loaded safely. Try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var query = SearchText.Trim();
        Accounts = query.Length == 0
            ? _allAccounts
            : _allAccounts
                .Where(account =>
                    account.Issuer.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || account.AccountName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToArray();
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
