using System.Threading.Tasks;

namespace Github2FA.Interfaces
{
    public interface IDelayService
    {
        Task Delay(int milliseconds);
    }

}
