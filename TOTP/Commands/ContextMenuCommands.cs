using Syncfusion.UI.Xaml.Grid;
using System.Windows.Input;

namespace TOTP.Commands;

public static class ContextMenuCommands
{
    private static ICommand? _cut;
    public static ICommand Cut => _cut ??= new BaseCommand(OnCutClicked, CanCut);

    private static bool CanCut(object? obj) =>
        obj is GridRecordContextMenuInfo contextInfo &&
        contextInfo.DataGrid is not null;

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
