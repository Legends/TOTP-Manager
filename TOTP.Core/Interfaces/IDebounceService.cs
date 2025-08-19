using System;

namespace TOTP.Core.Interfaces;

public interface IDebounceService
{
    void Debounce(string key, int milliseconds, Action action);
    void Cancel(string key);
}