using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 

namespace TOTP.Security;

public enum AuthorizationGateKind
{
    None = 0,
    WindowsHello = 1,
    Password = 2
}
