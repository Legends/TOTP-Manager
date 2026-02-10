namespace TOTP.Interfaces;

public interface IDelayService
{
    Task Delay(int milliseconds);
}