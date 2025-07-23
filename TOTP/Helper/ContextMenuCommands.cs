using Syncfusion.UI.Xaml.Grid;
using System.Windows.Input;
using TOTP.Commands;

namespace TOTP.Helper;

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

    private static void OnCutClicked(object? obj)
    {
        if (obj is GridRecordContextMenuInfo contextInfo && contextInfo.DataGrid is not null)
        {
            var grid = contextInfo.DataGrid;
            var copypasteoption = grid.GridCopyOption;
            grid.GridCopyOption = GridCopyOption.CutData;
            grid.GridCopyPaste.Cut();
            grid.GridCopyOption = copypasteoption;
        }
    }

}
