using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace TOTP.Avalonia.Desktop.Presentation.Dialogs;

public sealed class PasswordDialogViewModel : INotifyPropertyChanged, IDisposable
{
    private Func<string, CancellationToken, Task<string?>>? _validateAsync;
    private readonly CancellationToken _cancellationToken;
    private readonly AsyncCommand _confirmCommand;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;
    private bool _closeRequested;
    private bool _disposed;

    public PasswordDialogViewModel(
        PasswordDialogRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);
        Title = request.Title;
        Message = request.Message;
        ConfirmText = request.ConfirmText;
        CancelText = request.CancelText;
        RequiredMessage = request.RequiredMessage;
        ValidationFailureMessage = request.ValidationFailureMessage;
        _validateAsync = request.ValidateAsync;
        _cancellationToken = cancellationToken;
        _confirmCommand = new AsyncCommand(ConfirmAsync, () => !_isBusy && !_closeRequested);
        CancelCommand = new AsyncCommand(CancelAsync, () => !_isBusy && !_closeRequested);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<string?>? CloseRequested;

    public string Title { get; }
    public string Message { get; }
    public string ConfirmText { get; }
    public string CancelText { get; }
    public string RequiredMessage { get; }
    public string ValidationFailureMessage { get; }

    public string Password
    {
        get => _password;
        set
        {
            if (!SetField(ref _password, value ?? string.Empty)) return;
            ErrorMessage = string.Empty;
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetField(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetField(ref _isBusy, value)) return;
            _confirmCommand.NotifyCanExecuteChanged();
            if (CancelCommand is AsyncCommand cancelCommand)
                cancelCommand.NotifyCanExecuteChanged();
        }
    }

    public ICommand ConfirmCommand => _confirmCommand;
    public ICommand CancelCommand { get; }

    public async Task ConfirmAsync()
    {
        if (IsBusy || _closeRequested || _disposed) return;

        var candidate = Password;
        Password = string.Empty;
        if (string.IsNullOrWhiteSpace(candidate))
        {
            ErrorMessage = RequiredMessage;
            return;
        }

        IsBusy = true;
        try
        {
            if (_validateAsync is not null)
            {
                var validationError = await _validateAsync(candidate, _cancellationToken);
                if (!string.IsNullOrWhiteSpace(validationError))
                {
                    ErrorMessage = validationError;
                    return;
                }
            }

            RequestClose(candidate);
        }
        catch (OperationCanceledException) when (_cancellationToken.IsCancellationRequested)
        {
            RequestClose(null);
        }
        catch (Exception)
        {
            ErrorMessage = ValidationFailureMessage;
        }
        finally
        {
            candidate = string.Empty;
            IsBusy = false;
        }
    }

    public void Cancel()
    {
        if (IsBusy || _closeRequested || _disposed) return;
        Password = string.Empty;
        RequestClose(null);
    }

    public void ClearSensitiveData()
    {
        _password = string.Empty;
        _errorMessage = string.Empty;
        _validateAsync = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        ClearSensitiveData();
    }

    private Task CancelAsync()
    {
        Cancel();
        return Task.CompletedTask;
    }

    private void RequestClose(string? password)
    {
        if (_closeRequested) return;
        _closeRequested = true;
        _confirmCommand.NotifyCanExecuteChanged();
        if (CancelCommand is AsyncCommand cancelCommand)
            cancelCommand.NotifyCanExecuteChanged();
        CloseRequested?.Invoke(password);
    }

    private static void ValidateRequest(PasswordDialogRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title)
            || string.IsNullOrWhiteSpace(request.Message)
            || string.IsNullOrWhiteSpace(request.ConfirmText)
            || string.IsNullOrWhiteSpace(request.CancelText)
            || string.IsNullOrWhiteSpace(request.RequiredMessage)
            || string.IsNullOrWhiteSpace(request.ValidationFailureMessage))
        {
            throw new ArgumentException("Password dialog text must be explicit and non-empty.", nameof(request));
        }
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
