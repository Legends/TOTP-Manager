using Microsoft.Xaml.Behaviors;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TOTP.Behaviors;

public class AsyncInvokeCommandAction : TriggerAction<DependencyObject>
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(AsyncInvokeCommandAction), new PropertyMetadata(null));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override async void Invoke(object parameter)
    {
        if (Command == null)
            return;

        try
        {
            if (Command is IAsyncCommand asyncCommand)
            {
                await asyncCommand.ExecuteAsync(parameter);
            }
            else if (Command.CanExecute(parameter))
            {
                Command.Execute(parameter);
            }
        }
        catch (Exception ex)
        {
            // Optional: log or show error message
            Console.WriteLine($"Exception in AsyncInvokeCommandAction: {ex.Message}");
        }
    }
}

public interface IAsyncCommand : ICommand
{
    Task ExecuteAsync(object? parameter);
}
