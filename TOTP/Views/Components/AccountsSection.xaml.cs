using Syncfusion.UI.Xaml.Grid;
using System.Windows.Controls;

namespace TOTP.Views.Components;

public partial class AccountsSection : UserControl
{
    public AccountsSection()
    {
        InitializeComponent();
    }

    public SfDataGrid AccountsGridControl => AccountsGrid;
}
