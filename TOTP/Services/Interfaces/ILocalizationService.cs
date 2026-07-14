using System;

namespace TOTP.Services.Interfaces;

public interface ILocalizationService
{
    event Action? LanguageChanged;

    void ApplyCurrentCulture();

    void ChangeCulture(string cultureName);
}
