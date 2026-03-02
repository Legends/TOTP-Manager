using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TOTP.Core.Interfaces;

public interface IDispatcherService
{
    void InvokeOnUI(Action action);
    bool CheckAccess();
}
