using System;

namespace Github2FA.Interfaces
{
    public interface IErrorHandler
    {
        void Handle(Exception exception, string userMessage);
    }
}
