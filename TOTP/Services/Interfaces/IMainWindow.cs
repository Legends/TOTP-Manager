using System.Windows.Input;

namespace TOTP.Services.Interfaces;

public interface IMainWindow
{
    bool IsActive { get; }

    event MouseButtonEventHandler PreviewMouseDown;
    event MouseWheelEventHandler PreviewMouseWheel;
    event KeyEventHandler PreviewKeyDown;
    event TextCompositionEventHandler PreviewTextInput;
    event MouseEventHandler PreviewMouseMove;
}
