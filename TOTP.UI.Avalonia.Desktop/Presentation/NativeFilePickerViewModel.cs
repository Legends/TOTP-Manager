using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TOTP.Avalonia.Desktop.Platform;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class NativeFilePickerViewModel : INotifyPropertyChanged
{
    private readonly IAvaloniaFilePicker _filePicker;
    private readonly AsyncCommand _pickCommand;
    private string _message = string.Empty;
    private bool _isBusy;

    public NativeFilePickerViewModel(IAvaloniaFilePicker filePicker)
    {
        _filePicker = filePicker ?? throw new ArgumentNullException(nameof(filePicker));
        _pickCommand = new AsyncCommand(PickAsync, () => !_isBusy);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Message
    {
        get => _message;
        private set => SetField(ref _message, value);
    }

    public ICommand PickCommand => _pickCommand;

    public async Task PickAsync()
    {
        if (_isBusy) return;
        _isBusy = true;
        _pickCommand.NotifyCanExecuteChanged();
        try
        {
            var name = await _filePicker.PickImportFileNameAsync();
            Message = name is null
                ? "No import file selected."
                : $"Selected {name}. Import is not enabled in this technical preview.";
        }
        catch (Exception)
        {
            Message = "The native file picker could not be opened safely.";
        }
        finally
        {
            _isBusy = false;
            _pickCommand.NotifyCanExecuteChanged();
        }
    }

    private void SetField(ref string field, string value, [CallerMemberName] string? propertyName = null)
    {
        if (field == value) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
