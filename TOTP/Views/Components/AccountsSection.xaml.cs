using Syncfusion.UI.Xaml.Grid;
using System.Windows.Controls;

namespace TOTP.Views.Components;

public partial class TokensSection : UserControl
{
    public TokensSection()
    {
        InitializeComponent();
    }

    public SfDataGrid TokensGridControl => TokensGrid;
}
