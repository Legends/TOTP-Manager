using Avalonia.Threading;
using TOTP.Avalonia.Desktop.Dialogs;
using TOTP.Avalonia.Desktop.Presentation.Dialogs;

namespace TOTP.Avalonia.Desktop.Platform;

public sealed class AvaloniaDialogService(AvaloniaWindowCoordinator windows) : IAvaloniaDialogService
{
    private readonly SemaphoreSlim _dialogGate = new(1, 1);

    public async Task<bool> ConfirmAsync(
        ConfirmationDialogRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _dialogGate.WaitAsync(cancellationToken);
        try
        {
            var owner = windows.GetRequiredMainWindow();
            var viewModel = new ConfirmationDialogViewModel(request);
            var dialog = new ConfirmationDialogWindow { DataContext = viewModel };
            var requestedResult = false;
            viewModel.CloseRequested += Close;
            using var ownership = windows.RegisterOwnedDialog(dialog);
            using var cancellation = cancellationToken.Register(
                () => Dispatcher.UIThread.Post(() => dialog.Close(false)));

            try
            {
                var result = await dialog.ShowDialog<bool>(owner);
                return result && requestedResult;
            }
            finally
            {
                viewModel.CloseRequested -= Close;
                dialog.DataContext = null;
            }

            void Close(bool result)
            {
                requestedResult = result;
                dialog.Close(result);
            }
        }
        finally
        {
            _dialogGate.Release();
        }
    }

    public async Task<string?> PromptForPasswordAsync(
        PasswordDialogRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _dialogGate.WaitAsync(cancellationToken);
        try
        {
            var owner = windows.GetRequiredMainWindow();
            using var viewModel = new PasswordDialogViewModel(request, cancellationToken);
            var dialog = new PasswordDialogWindow { DataContext = viewModel };
            string? confirmedPassword = null;
            viewModel.CloseRequested += Close;
            using var ownership = windows.RegisterOwnedDialog(dialog);
            using var cancellation = cancellationToken.Register(
                () => Dispatcher.UIThread.Post(() => dialog.Close(false)));

            try
            {
                var result = await dialog.ShowDialog<bool>(owner);
                return result ? confirmedPassword : null;
            }
            finally
            {
                viewModel.CloseRequested -= Close;
                viewModel.ClearSensitiveData();
                dialog.DataContext = null;
            }

            void Close(string? password)
            {
                confirmedPassword = password;
                dialog.Close(password is not null);
            }
        }
        finally
        {
            _dialogGate.Release();
        }
    }
}
