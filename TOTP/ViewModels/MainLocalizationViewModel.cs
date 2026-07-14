using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using TOTP.Presentation;
using TOTP.Services.Interfaces;
using TOTP.ViewModels.Models;

namespace TOTP.ViewModels;

public sealed class MainLocalizationViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly ILocalizationService _localizationService;
    private CultureDisplay _selectedCulture;

    public MainLocalizationViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        SupportedCultures =
        [
            new(new CultureInfo("en"), PresentationConstants.EnglishFlagUri),
            new(new CultureInfo("de-DE"), PresentationConstants.GermanFlagUri),
        ];
        _selectedCulture = FindCurrentCulture();
        _localizationService.LanguageChanged += OnLanguageChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action? LanguageChanged;

    public ObservableCollection<CultureDisplay> SupportedCultures { get; }

    public CultureDisplay SelectedCulture
    {
        get => _selectedCulture;
        set
        {
            if (ReferenceEquals(_selectedCulture, value))
                return;

            _selectedCulture = value;
            OnPropertyChanged();
            _localizationService.ChangeCulture(value.Culture.Name);
        }
    }

    private void OnLanguageChanged()
    {
        var current = FindCurrentCulture();
        if (!ReferenceEquals(_selectedCulture, current))
        {
            _selectedCulture = current;
            OnPropertyChanged(nameof(SelectedCulture));
        }

        LanguageChanged?.Invoke();
    }

    private CultureDisplay FindCurrentCulture()
        => SupportedCultures.FirstOrDefault(c =>
               c.Culture.Name == CultureInfo.CurrentUICulture.Name)
           ?? SupportedCultures.First();

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public void Dispose()
        => _localizationService.LanguageChanged -= OnLanguageChanged;
}
