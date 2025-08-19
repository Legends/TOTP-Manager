using System;

namespace TOTP.Interfaces;

public interface IDebounceService
{
    void Debounce(string key, int milliseconds, Action action);
    void Cancel(string key);
}