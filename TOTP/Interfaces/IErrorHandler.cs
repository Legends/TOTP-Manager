using System;

namespace TOTP.Interfaces;

public interface IErrorHandler
{
    void Handle(Exception exception, string userMessage);
}