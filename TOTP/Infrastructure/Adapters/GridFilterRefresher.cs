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
    private Predicate<object>? _pendingFilter;
    private bool _pendingRefresh;

    public GridFilterRefresher(SfDataGrid grid)
    {
        _grid = grid;
        _grid.Loaded += (_, __) => ApplyPendingState();
    }

    public void Refresh()
    {
        _pendingRefresh = true;
        ApplyPendingState();
    }


    public void ApplySearchFilter(Predicate<object> filter)
    {
        _pendingFilter = filter;
        ApplyPendingState();
    }

    private void ApplyPendingState()
    {
        void Apply()
        {
            if (_grid.View == null)
            {
                return;
            }

            if (_pendingFilter != null)
            {
                _grid.View.Filter = _pendingFilter;
            }

            if (_pendingRefresh)
            {
                _grid.View.RefreshFilter();
                _pendingRefresh = false;
            }
        }

        if (_grid.Dispatcher.CheckAccess())
        {
            Apply();
            return;
        }

        _grid.Dispatcher.Invoke(Apply);
    }
}
