using System;

namespace TOTP.Core.Interfaces;

public interface IErrorHandler
{
    void Handle(Exception exception, string userMessage);
}