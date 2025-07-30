using System;

namespace TOTP.Events
{
    public class DialogCloseRequestedEventArgs : EventArgs
    {
        public bool DialogResult { get; }

        public DialogCloseRequestedEventArgs(bool dialogResult)
        {
            DialogResult = dialogResult;
        }
    }

}
