using Syncfusion.UI.Xaml.Grid;
using System.Windows.Input;
using TOTP.Commands;

namespace TOTP.Helper;

public static class ContextMenuCommands
{
    private static ICommand? cut;
    public static ICommand Cut => cut ??= new BaseCommand(OnCutClicked);


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