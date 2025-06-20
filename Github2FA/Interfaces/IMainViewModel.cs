using System.Windows.Input;

namespace Github2FA.Interfaces
{
    public interface IMainViewModel
    {
        ICommand AddNewTotpCommand { get; }
        ICommand Cmd2Command { get; }
        ICommand Cmd3Command { get; }
    }
}