using System;

namespace TOTP.Core.Events
{
    public class AddNewPromptArgs : EventArgs
    {
        public bool Success { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
    }
}
