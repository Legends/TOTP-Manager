using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System.Windows;
using System.Windows.Input;

namespace TOTP.Behaviors
{
    public class RowClickBehavior : Behavior<SfDataGrid>
    {
        public static readonly DependencyProperty SelectionChangedCommandProperty =
            DependencyProperty.Register(nameof(SelectionChangedCommand), typeof(ICommand),
                typeof(RowClickBehavior), new PropertyMetadata(null));




        public ICommand SelectionChangedCommand
        {
            get => (ICommand)GetValue(SelectionChangedCommandProperty);
            set => SetValue(SelectionChangedCommandProperty, value);
        }


        protected override void OnAttached()
        {
            AssociatedObject.PreviewMouseDown += OnPreviewMouseDown;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseDown -= OnPreviewMouseDown;
            base.OnDetaching();
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var grid = AssociatedObject;
            if (grid == null) return;

            // Hit-test on the VisualContainer to get row/column under mouse
            var vc = grid.GetVisualContainer();
            var rc = vc.PointToCellRowColumnIndex(e.GetPosition(vc));
            if (!rc.IsEmpty && rc.RowIndex > 0 && grid.View != null)
            {
                int recordIndex = grid.ResolveToRecordIndex(rc.RowIndex);
                if (recordIndex >= 0 && recordIndex < grid.View.Records.Count)
                {
                    var item = grid.View.Records[recordIndex].Data;  // <-- not null
                    // do stuff with item

                    if (item == null) return;

                    if (e.ChangedButton == MouseButton.Right)
                    {
                        // Ensure the row becomes selected before context menu opens
                        grid.SelectedItem = item;
                        grid.CurrentItem = item;

                        //var menu = grid.RecordContextMenu;
                        //if (menu == null) return;

                        //menu.PlacementTarget = grid;                // lets us bind via PlacementTarget in XAML
                        //menu.Placement = PlacementMode.MousePoint;  // show at cursor
                        //menu.IsOpen = true;

                        //if (RightClickCommand != null && RightClickCommand.CanExecute(item))
                        //    RightClickCommand.Execute(item);

                        // Let the ContextMenu open normally
                    }
                    else if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
                    {
                        grid.SelectedItem = item;
                        grid.CurrentItem = item;

                        if (SelectionChangedCommand != null && SelectionChangedCommand.CanExecute(item))
                            SelectionChangedCommand.Execute(item);
                    }
                }
            }


        }
    }
}
