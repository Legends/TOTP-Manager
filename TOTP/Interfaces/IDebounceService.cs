using System;

namespace Github2FA.Interfaces
{
    public interface IDebounceService
    {
        void Debounce(string key, int milliseconds, Action action);
        void Cancel(string key);
    }

}
