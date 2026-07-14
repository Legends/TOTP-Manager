using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TOTP.Presentation.Services.Interfaces;

public interface IDispatcherService
{
    void InvokeOnUI(Action action);
    Task InvokeAsync(Func<Task> action);
    bool CheckAccess();
}
