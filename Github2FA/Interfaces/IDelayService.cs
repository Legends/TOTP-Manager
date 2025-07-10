using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Github2FA.Interfaces
{
    public interface IDelayService
    {
        Task Delay(int milliseconds);
    }

}
