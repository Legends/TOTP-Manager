using System.Threading.Tasks;
using TOTP.Interfaces;

namespace TOTP.Services;

public class DelayService : IDelayService
{
    public Task Delay(int milliseconds)
    {
        return Task.Delay(milliseconds);
    }
}