using System.Windows;

namespace TOTP.Services.Interfaces;

public interface IInputActivityMonitor
{
    void Attach(Window window);
    void Detach();
}
