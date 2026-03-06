using OtpNet;
using Syncfusion.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TOTP.Infrastructure.Parser;
using TOTP.Resources;
using TOTP.Services.Interfaces;
using TOTP.Validation;
using TOTP.ViewModels.Interfaces;

namespace TOTP.ViewModels;

public partial class MainViewModel
{
    #region ### Row/Field Grid Selection  ###

    private bool _isDoubleClick;

    public async Task OnRowSelectionChangedAsync(OtpViewModel? selectedSecretItem)
    {
        if (selectedSecretItem == null)
        {
            Debug.WriteLine("OnRowSelectionChangedAsync - early return");
            return;
        }

        if (SelectedToken != null && IsInlineEditing && SelectedToken.ID != selectedSecretItem.ID)
            IsInlineEditing = false;

        if (IsGridEditing || IsInlineEditing)
        {
            Debug.WriteLine("OnRowSelectionChangedAsync - early return");
            return;
        }

        _isDoubleClick = false;
        await Task.Delay(300);

        if (_isDoubleClick)
        {
            if (SelectedToken == null)
                TotpUiTimer?.Dispose();

            return;
        }

        if (SelectedToken?.ID == selectedSecretItem.ID)
            return;

        SelectedToken = ComputeTotpCode(selectedSecretItem, out _activeTotp);
        CopyTotpCodeToClipboard();

        var currentKey = SelectedToken.Issuer;

        try
        {
            if (currentKey == SelectedToken.Issuer)
            {
                if (SelectedToken != null && !SelectedToken.IsBeingEdited && !IsContextmenuOpen)
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
        finally
        {
            _isDoubleClick = false;
        }
    }

    #endregion

    #region ### TOTP Code Generation ###

    // Secret: JBSWY3DPEHPK3PXP
    public OtpViewModel ComputeTotpCode(OtpViewModel item, out Totp totpInstance)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.Secret) || !UiValidation.IsValidBase32Format(item.Secret))
            throw new FormatException($"Secret is invalid Base32 format, supplied to {nameof(ComputeTotpCode)}");

        var encodedSecret = Base32Encoding.ToBytes(item.Secret);
        totpInstance = new Totp(encodedSecret);

        TotpCode = totpInstance.ComputeTotp();
        RemainingSeconds = totpInstance.RemainingSeconds();

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
        ShowCopySymbol = true;

        ShowCodeGenerationOutput();
    }

    private void StartTotpTick()
    {
        TotpUiTimer?.Dispose();
        TotpUiTimer = new System.Threading.Timer(_ =>
        {
            if (_activeTotp is null || SelectedToken is null)
            {
                return;
            }

            Debug.WriteLine("#######  Timer is running  #####");

            if (_activeTotp is null || SelectedToken is null) throw new NullReferenceException(nameof(_activeTotp));

            const int period = 30;
            long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long step = unix / period;

            _activeStep = step;
            var now = DateTime.UtcNow;

            Application.Current?.Dispatcher.BeginInvoke(
               DispatcherPriority.Render,
               new Action(() =>
               {
                   TotpCode = _activeTotp.ComputeTotp();
                   RemainingSeconds = _activeTotp.RemainingSeconds();
               }));

        }, null, dueTime: 0, period: 800);
    }

    private BitmapImage GenerateQRCodeImage(OtpViewModel item)
    {
        if (string.IsNullOrWhiteSpace(item.Secret))
            throw new FormatException("Secret is required for QR generation.");

        var normalizedSecret = OtpauthParser.NormalizeBase32SecretForUri(item.Secret);
        var issuer = item.Issuer ?? string.Empty;
        var uri = _qrService.BuildOtpAuthUri(issuer, normalizedSecret, item.TokenName);
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

    private static int _counter;
    public static int Increment()
    {
        return Interlocked.Increment(ref _counter);
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
        if (SelectedToken == null)
            return;

        var bmp = GenerateQRCodeImage(SelectedToken);
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
        if (!clearEnabled)
        {
            _clipboardService.SetText(TotpCode);
            ShowCopySymbol = true;
            return;
        }

        var seconds = _settingsService.Current.ClearClipboardSeconds > 0
            ? _settingsService.Current.ClearClipboardSeconds
            : 15;

        _clipboardService.CopyAndScheduleClear(TotpCode, TimeSpan.FromSeconds(seconds));
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
