namespace TOTP.Core.Services.Interfaces;

public interface IDelayService
{
    Task Delay(int milliseconds);
}