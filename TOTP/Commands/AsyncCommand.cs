using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TOTP.Commands
{
    // The Base Generic Version handles all the heavy lifting
    public class AsyncCommand<T> : ICommand
    {
        private readonly Func<T?, Task> _execute;
        private readonly Predicate<T?>? _canExecute;
        private readonly ILogger? _logger;
        private bool _isExecuting;

        public event EventHandler? CanExecuteChanged;

        public AsyncCommand(Func<T?, Task> execute,
                            Predicate<T?>? canExecute = null,
                            ILogger? logger = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            _logger = logger;
        }

        public bool CanExecute(object? parameter) =>
            !_isExecuting && (_canExecute?.Invoke((T?)parameter) ?? true);

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();
                await _execute((T?)parameter);
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.LogError(ex, "Unhandled exception in AsyncCommand");
                }
                else
                {
                    CommandExceptionLogger.LogUnhandled(nameof(AsyncCommand), ex);
                }
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    // The Non-Generic version now just inherits and passes "object"
    public class AsyncCommand : AsyncCommand<object>
    {
        public AsyncCommand(Func<Task> execute, Func<bool>? canExecute = null, ILogger? logger = null)
            : base(_ => execute(), canExecute is null ? null : _ => canExecute(), logger)
        { }
    }
}
