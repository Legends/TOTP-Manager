using TOTP.Interfaces;
using System.Threading.Tasks;

namespace TOTP.Services;

public class DelayService : IDelayService
{
    public Task Delay(int milliseconds) => Task.Delay(milliseconds);
}
