using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TOTP.Presentation.Adapters;

public interface IGridFilterRefresher
{
    void Refresh();
    void ApplySearchFilter(Predicate<object> filter);

}

