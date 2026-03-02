using TOTP.Core.Services.Interfaces;

namespace TOTP.Infrastructure.Services;

public class DelayService : IDelayService
{
    public Task Delay(int milliseconds)
    {
        return Task.Delay(milliseconds);
    }
}