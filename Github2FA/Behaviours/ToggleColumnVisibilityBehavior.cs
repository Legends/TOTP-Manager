
using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using System.Linq;
using System.Windows;

namespace Github2FA.Behaviors
{
    public class ToggleColumnVisibilityBehavior : Behavior<SfDataGrid>
    {
        public static readonly DependencyProperty ShowActionsColumnProperty =
            DependencyProperty.Register(nameof(ShowActionsColumn), typeof(bool), typeof(ToggleColumnVisibilityBehavior),
                new PropertyMetadata(false, OnShowActionsColumnChanged));

        public bool ShowActionsColumn
        {
            get => (bool)GetValue(ShowActionsColumnProperty);
            set => SetValue(ShowActionsColumnProperty, value);
        }

        private static void OnShowActionsColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToggleColumnVisibilityBehavior behavior && behavior.AssociatedObject != null)
            {
                behavior.UpdateColumnVisibility();
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            UpdateColumnVisibility();
        }

        private void UpdateColumnVisibility()
        {
            var grid = AssociatedObject;
            var actionsColumn = grid.Columns.FirstOrDefault(c => c.HeaderText == "Actions");
            if (actionsColumn != null)
            {
                actionsColumn.IsHidden = !ShowActionsColumn;
            }
        }
    }
}