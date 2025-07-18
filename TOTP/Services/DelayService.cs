using Github2FA.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Github2FA.Services
{
    public class DelayService : IDelayService
    {
        public Task Delay(int milliseconds) => Task.Delay(milliseconds);
    }

}
