using Github2FA.Interfaces;
using Syncfusion.Windows.Shared;
using System.Windows;

namespace Github2FA.Services
{
  

    public class MessageService : IMessageService
    {
        public void ShowMessage(string message, string caption = "Info")
        {
            System.Windows.MessageBox.Show(message, caption);
        }
    }

}
