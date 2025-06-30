using Github2FA.Interfaces;
using Github2FA.Models;
using Github2FA.ViewModels;
using Microsoft.Extensions.Configuration;
using OtpNet;
using Syncfusion.PMML;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace Github2FA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SecretItem? _originalSecret;
        IMainViewModel _vm;

        public MainWindow(IMainViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            _vm = vm;
        }

        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{

        //    ActionsColumn.IsHidden = !_vm.ShowActionsColumn;

        //    _vm.PropertyChanged += (s, args) =>
        //    {
        //        if (args.PropertyName == nameof(_vm.ShowActionsColumn))
        //        {
        //            ActionsColumn.IsHidden = !_vm.ShowActionsColumn;
        //        }
        //    };
        //}


        private void SecretsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is SfDataGrid grid)
            {
                var rowData = grid.SelectedItem as SecretItem;
                if (rowData == null)
                    return;

                // Optional: deactivate edit mode for all other rows
                foreach (var item in _vm.Secrets)
                    item.IsBeingEdited = false;

                // Toggle edit mode for the clicked row
                rowData.IsBeingEdited = !rowData.IsBeingEdited;
            }
        }

        private void SecretsGrid_CurrentCellBeginEdit(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellBeginEditEventArgs e)
        {
            ActionsColumn.IsHidden = false;
            if (SecretsGrid.SelectedItem is SecretItem item)
            {
                // Create a deep copy (or shallow if sufficient)
                _originalSecret = new SecretItem(item.Key, item.Value);
                _vm.PreviousVersion = new SecretItem(item.Key, item.Value);
                item.IsBeingEdited = true;
            }

        }

        private void SecretsGrid_CurrentCellEndEdit(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellEndEditEventArgs e)
        {
            ActionsColumn.IsHidden = true;


            if (SecretsGrid.View.CurrentItem is SecretItem item)
                item.IsBeingEdited = false;

            if ((e.OriginalSender as SfDataGrid)?.CurrentItem is SecretItem updatedSecret)
            {
                if (!updatedSecret.Equals(_originalSecret))
                {
                    _vm.UpdateSecret(_originalSecret, updatedSecret);
                }                 
            }
            _originalSecret = null;
            _vm.PreviousVersion = null;
        }
    }
}
