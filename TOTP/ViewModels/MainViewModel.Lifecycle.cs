using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TOTP.Core.Enums;
using TOTP.Core.Security.Models;
using TOTP.Infrastructure.Extensions;
using TOTP.Infrastructure.Common;
using TOTP.Services;
using TOTP.Validation;
using TOTP.ViewModels.Interfaces;

namespace TOTP.ViewModels;

public partial class MainViewModel
{
    #region ### LOCALIZATION SETUP ###

    private void SetupLocalization()
    {
        SupportedCultures =
        [
            new(new CultureInfo("en"), StringsConstants.ImgUrl.EnFlag),
            new(new CultureInfo("de-DE"), StringsConstants.ImgUrl.DeFlag),
        ];

        var currentCulture = CultureInfo.CurrentUICulture;

        var selCulture = SupportedCultures.FirstOrDefault(c => c.Culture.Name == currentCulture.Name)
                         ?? SupportedCultures.First();
        SelectedCulture = selCulture;

        LocalizationService.LanguageChanged += LocalizationService_LanguageChanged;
    }

    private void LocalizationService_LanguageChanged()
    {
        OnPropertyChanged(nameof(ExportToolTip));
    }

    #endregion

    private async Task EnsureTokensLoadedAsync()
    {
        if (_otpLoadedFromStore)
            return;

        await ReadAllOtpsAsync();

        GridFilterRefresher.ApplySearchFilter(((IMainViewModel)this).DoFilterGrid);

        if (!_collectionHooked)
        {
            AllOtps.CollectionChanged += Source_CollectionChanged;
            _collectionHooked = true;
        }

        _otpLoadedFromStore = true;
    }

    private void Source_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (OtpViewModel item in e.NewItems)
            {
                item.SetDuplicateCheck(DuplicateCheck);
            }
        }
    }

    private ValidationError DuplicateCheck(OtpViewModel si)
    {
        return _accountsWorkflow.CheckDuplicateIssuer(si, AllOtps);
    }

    #region ### AUTHORIZATION ###

    private void SessionController_SessionStateChanged(object? sender, AppSessionLockState state)
    {
        SessionState = state;
    }

    private async Task OnUnlockedAsync()
    {
        await EnsureTokensLoadedAsync();
        _ = WarmUpNonCriticalFeaturesAsync();
    }

    private void OnLocked()
    {
        _debounceService.Cancel("Search");
        _qrPreviewService.Close();
        _otpLoadedFromStore = false;
        AllOtps.Clear();

        StopTotpTimer();
        ClearCodeGenerationOutput();
        ClearSearchTextbox();
        CancelFlyout();
        IsSettingsViewOpen = false;
        IsSecretVisible = false;
        IsGridEditing = false;
        IsInlineEditing = false;
        SelectedAccount = null;
    }

    #endregion

    #region ### OnPropertyChanged ###

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        try
        {
            _debounceService.Cancel("Search");
        }
        catch
        {
            // best-effort cleanup
        }

        LocalizationService.LanguageChanged -= LocalizationService_LanguageChanged;
        _mainViewSessionController.SessionStateChanged -= SessionController_SessionStateChanged;

        if (_collectionHooked)
        {
            AllOtps.CollectionChanged -= Source_CollectionChanged;
            _collectionHooked = false;
        }

        StopTotpTimer();
    }

    #endregion

    #region ### READ ALL OTP ENTRIES FROM STORAGE FILE ###

    private async Task ReadAllOtpsAsync()
    {
        try
        {
            var result = await _accountsWorkflow.LoadAllAsync();

            if (result.IsFailed)
            {
                _messageService.ShowResultError(result);
                return;
            }

            AllOtps = result.Value;

            foreach (var item in AllOtps)
            {
                item.SetDuplicateCheck(DuplicateCheck);
            }

        }
        catch (Exception e)
        {
            _logger.LogCritical(e, nameof(ReadAllOtpsAsync));
            System.Windows.Application.Current.Shutdown(1);
        }

    }

    #endregion
}
