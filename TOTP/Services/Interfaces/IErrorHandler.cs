using System;

namespace TOTP.Services.Interfaces;

public interface IErrorHandler
{
    void Handle(Exception exception, string userMessage);
}