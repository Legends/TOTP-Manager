using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using System.Windows.Media.Imaging;
using TOTP.Infrastructure.Parser;
using TOTP.Resources;
using TOTP.Services.Interfaces;
using TOTP.Validation;
using TOTP.ViewModels.Interfaces;

namespace TOTP.ViewModels;

public partial class MainViewModel
{
    #region ### Row/Field Grid Selection  ###

    public Task OnRowSelectionChangedAsync(OtpViewModel? selectedSecretItem)
    {
        if (selectedSecretItem == null)
        {
            Debug.WriteLine("OnRowSelectionChangedAsync - early return");
            return Task.CompletedTask;
        }

        if (SelectedAccount != null && IsInlineEditing && SelectedAccount.ID != selectedSecretItem.ID)
            IsInlineEditing = false;

        if (IsGridEditing || IsInlineEditing)
        {
            Debug.WriteLine("OnRowSelectionChangedAsync - early return");
            return Task.CompletedTask;
        }

        if (SelectedAccount?.ID == selectedSecretItem.ID)
            return Task.CompletedTask;

        SelectedAccount = ComputeTotpCode(selectedSecretItem);
        CopyTotpCodeToClipboard();

        var currentKey = SelectedAccount.Issuer;

        try
        {
            if (currentKey == SelectedAccount.Issuer)
            {
                if (SelectedAccount != null && !SelectedAccount.IsBeingEdited && !IsContextmenuOpen)
                    try
                    {
                        OnRowSelectionImplementation();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, UI.ex_Error_Generating_TOTP);
                        _messageService.ShowError(UI.ex_Error_Generating_TOTP);
                    }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, UI.ex_Selecting_Secret);
            _messageService.ShowError(UI.ex_Selecting_Secret + ": " + e.Message);
        }

        return Task.CompletedTask;
    }

    #endregion

    #region ### TOTP Code Generation ###

    // Secret: JBSWY3DPEHPK3PXP
    public OtpViewModel ComputeTotpCode(OtpViewModel item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.Secret) || !UiValidation.IsValidBase32Format(item.Secret))
            throw new FormatException($"Secret is invalid Base32 format, supplied to {nameof(ComputeTotpCode)}");

        var generated = _totpGenerator.Generate(item.Secret);
        TotpCode = generated.Code;
        RemainingSeconds = generated.RemainingSeconds;

        return item;
    }

    private string _TotpCode = string.Empty;
    public string TotpCode
    {
        get => _TotpCode;
        set
        {
            _TotpCode = value;
            OnPropertyChanged();
        }
    }

    public int PeriodSeconds { get; } = 30;

    int _remainingSeconds;
    public int RemainingSeconds
    {
        get => _remainingSeconds;
        set
        {
            _remainingSeconds = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ElapsedSeconds));
        }
    }

    public int ElapsedSeconds => PeriodSeconds - RemainingSeconds;

    private void OnRowSelectionImplementation()
    {

        Debug.WriteLine("CalculateAndDisplayTotpCode");
        if (TotpUiTimer != null)
            TotpUiTimer.Dispose();

        ClearCodeGenerationOutput();
        StartTotpTick();

        IsProgressPieChartVisible = true;
        CopyTotpCodeToClipboard();

        ShowCodeGenerationOutput();
    }

    private void StartTotpTick()
    {
        TotpUiTimer?.Dispose();
        TotpUiTimer = new System.Threading.Timer(_ =>
        {
            if (SelectedAccount is null || string.IsNullOrWhiteSpace(SelectedAccount.Secret))
            {
                return;
            }

            Debug.WriteLine("#######  Timer is running  #####");

            const int period = 30;
            long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long step = unix / period;

            _activeStep = step;
            var now = DateTime.UtcNow;

            _dispatcherService.Post(
               () =>
               {
                   if (SelectedAccount is null || string.IsNullOrWhiteSpace(SelectedAccount.Secret))
                   {
                       return;
                   }

                   var generated = _totpGenerator.Generate(SelectedAccount.Secret);
                   TotpCode = generated.Code;
                   RemainingSeconds = generated.RemainingSeconds;
               });

        }, null, dueTime: 0, period: 800);
    }

    private BitmapImage GenerateQRCodeImage(OtpViewModel item)
    {
        if (string.IsNullOrWhiteSpace(item.Secret))
            throw new FormatException("Secret is required for QR generation.");

        var normalizedSecret = OtpauthParser.NormalizeBase32SecretForUri(item.Secret);
        var issuer = item.Issuer ?? string.Empty;
        var uri = _qrService.BuildOtpAuthUri(issuer, normalizedSecret, item.AccountName);
        byte[] pngBytes = _qrService.GenerateQr(uri);

        var bmp = new BitmapImage();
        using (var ms = new MemoryStream(pngBytes))
        {
            bmp.BeginInit();
            bmp.StreamSource = ms;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();
        }

        return bmp;
    }

    #endregion

    #region ### Grid Filter Logic ###

    private bool FilterSecrets(object obj)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        return obj is OtpViewModel vm && (vm.Issuer?.IndexOf(SearchText.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
    }

    bool IMainViewModel.DoFilterGrid(object obj)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        Debug.WriteLine("---  DoFilterGrid   ----");
        return obj is OtpViewModel vm && (vm.Issuer?.IndexOf(SearchText.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private void ExecuteSearch()
    {
        try
        {
            GridFilterRefresher?.Refresh();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_Filtering_Secrets);
            _messageService.ShowError(UI.ex_Filtering_Secrets);
        }
    }

    public string DeleteLabel => TOTP.Resources.UI.ui_btnDelete;
    public string EditLabel => TOTP.Resources.UI.ui_btnEdit;
    public string ExportToolTip => Resources.UI.ui_Export;

    #endregion

    #region ### QR ###

    private void GenerateQrCodeImage()
    {
        if (SelectedAccount == null)
            return;

        var bmp = GenerateQRCodeImage(SelectedAccount);
        QrCodeImage = bmp;
        ShowGenerateQrCodeLink = false;
        IsQrVisible = true;
    }

    #endregion

    void ClearSearchTextbox()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            IsSearchVisible = false;
        }

        SearchText = "";
        IsSearchFocused = false;
        IsSearchFocused = IsSearchVisible;

    }

    public void CopyTotpCodeToClipboard()
    {
        var clearEnabled = _settingsService.Current.ClearClipboardEnabled;
        Result result;
        if (!clearEnabled)
        {
            result = _clipboardService.SetText(TotpCode);
        }
        else
        {
            var seconds = _settingsService.Current.ClearClipboardSeconds > 0
                ? _settingsService.Current.ClearClipboardSeconds
                : 15;

            result = _clipboardService.CopyAndScheduleClear(TotpCode, TimeSpan.FromSeconds(seconds));
        }

        if (result.IsFailed)
        {
            ShowCopySymbol = false;
            _messageService.ShowResultError(result);
            return;
        }

        ShowCopySymbol = true;
    }

    private void ShowCodeGenerationOutput()
    {
        CodeLabelHeight = 40;
        IsCodeCopiedLabelVisible = true;
        IsCodeLabelVisible = true;
        IsQrVisible = true;
        ShowGenerateQrCodeLink = true;
    }

    private void ClearCodeGenerationOutput()
    {
        CodeLabelHeight = 0;
        IsCodeCopiedLabelVisible = false;
        IsQrVisible = false;
        IsProgressPieChartVisible = false;
        IsCodeLabelVisible = false;
        QrCodeImage = null;
        CurrentCodeLabel = string.Empty;
        ShowGenerateQrCodeLink = false;
    }

    void StopTotpTimer()
    {
        TotpUiTimer?.Dispose();
    }
}
