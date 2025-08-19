using System.Threading.Tasks;

namespace TOTP.Core.Interfaces;

public interface IDelayService
{
    Task Delay(int milliseconds);
}