using Github2FA.Commands;
using Syncfusion.UI.Xaml.Grid;
using System.Windows.Input;

namespace Github2FA.Helper;

public static class ContextMenuCommands
{

    static ICommand? cut;
    public static ICommand Cut
    {
        get
        {
            if (cut == null)
                cut = new BaseCommand(OnCutClicked);

            return cut;
        }
    }

    private static void OnCutClicked(object obj)
    {
        if (obj is GridRecordContextMenuInfo)
        {
            var grid = (obj as GridRecordContextMenuInfo).DataGrid;
            var copypasteoption = grid.GridCopyOption;
            grid.GridCopyOption = GridCopyOption.CutData;
            grid.GridCopyPaste.Cut();
            grid.GridCopyOption = copypasteoption;
        }
    }
}
