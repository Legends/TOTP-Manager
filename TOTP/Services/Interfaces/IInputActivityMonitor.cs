using TOTP.Views.Interfaces;

namespace TOTP.Services.Interfaces;

public interface IInputActivityMonitor
{
    void Attach(IMainWindow window);
    void Detach();
}
