using System;

namespace TOTP.Services.Interfaces;

public interface IDebounceService : IDisposable
{
    void Debounce(string key, int milliseconds, Action action);
    void Cancel(string key);
}
