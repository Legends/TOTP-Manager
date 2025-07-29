using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace TOTP.Interfaces;

public interface IKeyValueDialogViewModel
{
    ICommand OkCommand { get; }
    ICommand CancelCommand { get; }
    ImageSource? Icon { get; set; }
    string? Platform { get; set; }
    string? Secret { get; set; }
    string? Account { get; set; }
    event EventHandler<bool>? RequestClose;
    event PropertyChangedEventHandler? PropertyChanged;
}