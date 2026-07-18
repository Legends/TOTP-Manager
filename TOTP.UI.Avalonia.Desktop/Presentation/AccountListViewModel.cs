using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class AccountListViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IAccountManager _accountManager;
    private readonly IAccountTotpService _accountTotpService;
    private readonly AsyncCommand _loadCommand;
    private readonly AsyncCommand _generateCommand;
    private CancellationTokenSource? _codeLifetime;
    private IReadOnlyList<AccountListItemViewModel> _allAccounts = [];
    private IReadOnlyList<AccountListItemViewModel> _accounts = [];
    private string _message = string.Empty;
    private string _searchText = string.Empty;
    private AccountListItemViewModel? _selectedAccount;
    private string _generatedCode = string.Empty;
    private string _codeMessage = string.Empty;
    private bool _isBusy;
    private bool _isGenerating;

    public AccountListViewModel(
        IAccountManager accountManager,
        IAccountTotpService accountTotpService)
    {
        _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
        _accountTotpService = accountTotpService ?? throw new ArgumentNullException(nameof(accountTotpService));
        _loadCommand = new AsyncCommand(LoadAsync, () => !_isBusy);
        _generateCommand = new AsyncCommand(
            GenerateCodeAsync,
            () => !_isGenerating && _selectedAccount is not null);
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

    public AccountListItemViewModel? SelectedAccount
    {
        get => _selectedAccount;
        set
        {
            if (!SetField(ref _selectedAccount, value)) return;
            ClearGeneratedCode();
            _generateCommand.NotifyCanExecuteChanged();
        }
    }

    public string GeneratedCode
    {
        get => _generatedCode;
        private set
        {
            if (!SetField(ref _generatedCode, value)) return;
            OnPropertyChanged(nameof(HasGeneratedCode));
        }
    }

    public bool HasGeneratedCode => GeneratedCode.Length > 0;

    public string CodeMessage
    {
        get => _codeMessage;
        private set
        {
            if (!SetField(ref _codeMessage, value)) return;
            OnPropertyChanged(nameof(HasCodeMessage));
        }
    }

    public bool HasCodeMessage => CodeMessage.Length > 0;

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

    public ICommand GenerateCommand => _generateCommand;

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

    public async Task GenerateCodeAsync()
    {
        if (_selectedAccount is null || _isGenerating) return;

        _isGenerating = true;
        _generateCommand.NotifyCanExecuteChanged();
        ClearGeneratedCode();
        try
        {
            var result = await _accountTotpService.GenerateAsync(_selectedAccount.Id);
            if (result.IsFailed)
            {
                CodeMessage = "A code could not be generated for this account.";
                return;
            }

            GeneratedCode = result.Value.Code;
            CodeMessage = $"Valid for {result.Value.RemainingSeconds} seconds.";
            _codeLifetime = new CancellationTokenSource();
            _ = ClearGeneratedCodeAfterAsync(
                TimeSpan.FromSeconds(Math.Max(1, result.Value.RemainingSeconds)),
                _codeLifetime.Token);
        }
        catch (Exception)
        {
            CodeMessage = "A code could not be generated safely. Try again.";
        }
        finally
        {
            _isGenerating = false;
            _generateCommand.NotifyCanExecuteChanged();
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

    private async Task ClearGeneratedCodeAfterAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken);
            GeneratedCode = string.Empty;
            CodeMessage = "Code expired. Generate a new code.";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private void ClearGeneratedCode()
    {
        _codeLifetime?.Cancel();
        _codeLifetime?.Dispose();
        _codeLifetime = null;
        GeneratedCode = string.Empty;
        CodeMessage = string.Empty;
    }

    public void Dispose() => ClearGeneratedCode();

    public void Clear()
    {
        SelectedAccount = null;
        SearchText = string.Empty;
        _allAccounts = [];
        Accounts = [];
        Message = string.Empty;
        ClearGeneratedCode();
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
