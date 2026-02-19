using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TOTP.Core.Interfaces;

namespace TOTP.Infrastructure.Adapters;

public class GridFilterRefresher : IGridFilterRefresher
{
    private readonly SfDataGrid _grid;

    public GridFilterRefresher(SfDataGrid grid)
    {
        _grid = grid;
    }

    public void Refresh()
    {
        _grid.Dispatcher.Invoke(() => _grid.View.RefreshFilter());
    }


    public void ApplySearchFilter(Predicate<object> filter)
    {
        _grid.View.Filter = filter;
    }
}
