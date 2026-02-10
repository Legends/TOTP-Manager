using System;
using System.Windows.Input;

namespace TOTP.Commands;

public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool>? _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        if (_canExecute is null)
            return true;

        if (parameter is T t)
            return _canExecute(t);

        // allow null for reference types / Nullable<T>
        if (parameter is null && default(T) is null)
            return _canExecute(default!);

        return false;
    }

    public void Execute(object? parameter)
    {
        if (parameter is T t)
        {
            _execute(t);
            return;
        }

        if (parameter is null && default(T) is null)
        {
            _execute(default!);
            return;
        }

        throw new InvalidCastException($"Invalid command parameter. Expected {typeof(T)}, got {parameter?.GetType()}");
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    //public event EventHandler? CanExecuteChanged
    //{
    //    add => CommandManager.RequerySuggested += value;
    //    remove => CommandManager.RequerySuggested -= value;
    //}

    // Optional: force a refresh immediately
    //public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();

    //public void RaiseCanExecuteChanged()
    //    => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public sealed class RelayCommand : RelayCommand<object?>
{
    // Keeps compatibility with: new RelayCommand(() => ...)
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
        : base(
            _ => execute(),
            _ => canExecute?.Invoke() ?? true)
    {
        if (execute is null) throw new ArgumentNullException(nameof(execute));
    }

    // Keeps compatibility with: new RelayCommand(p => ...)
    // Your existing API uses Func<bool> (no parameter) for canExecute — preserved here.
    public RelayCommand(Action<object?> execute, Func<bool>? canExecute = null)
        : base(
            execute ?? throw new ArgumentNullException(nameof(execute)),
            _ => canExecute?.Invoke() ?? true)
    {
    }
}
