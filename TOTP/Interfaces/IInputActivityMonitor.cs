using System.Windows;

namespace TOTP.Interfaces;

public interface IInputActivityMonitor
{
    void Attach(Window window);
    void Detach();
}
