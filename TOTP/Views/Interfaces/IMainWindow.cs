using System.Windows.Input;

namespace TOTP.Views.Interfaces;

public interface IMainWindow
{
    bool IsActive { get; }

    void BringToFront();

    event MouseButtonEventHandler PreviewMouseDown;
    event MouseWheelEventHandler PreviewMouseWheel;
    event KeyEventHandler PreviewKeyDown;
    event TextCompositionEventHandler PreviewTextInput;
    event MouseEventHandler PreviewMouseMove;
}
