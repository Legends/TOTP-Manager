using OtpNet;
using Syncfusion.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TOTP.Infrastructure.Parser;
using TOTP.Resources;
using TOTP.Validation;
using TOTP.ViewModels.Interfaces;
using static TOTP.ViewModels.SettingsViewModel;

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

        if (SelectedAccount != null && IsInlineEditing && SelectedAccount.ID != selectedSecretItem.ID)
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
            if (SelectedAccount == null)
                TotpUiTimer?.Dispose();

            return;
        }

        if (SelectedAccount?.ID == selectedSecretItem.ID)
            return;

        SelectedAccount = ComputeTotpCode(selectedSecretItem, out _activeTotp);
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

    private readonly SettingsViewModelFactory _settingsFactory;
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
            if (_activeTotp is null || SelectedAccount is null)
            {
                return;
            }

            Debug.WriteLine("#######  Timer is running  #####");

            if (_activeTotp is null || SelectedAccount is null) throw new NullReferenceException(nameof(_activeTotp));

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
        if (SelectedAccount == null)
            return;

        var bmp = GenerateQRCodeImage(SelectedAccount);
        QrCodeImage = bmp;
        ShowGenerateQrCodeLink = false;
        IsQrVisible = true;
    }

    #endregion

    #region ### EXPORT SECRETS TO EXTERNAL FILE ###

    private async Task ExportSecretsToFile()
    {
        try
        {
            var path = _fileDialogService.ShowSaveFileDialog(".txt|.json", ".json", "Totp-Accounts");

            if (path == null)
                return;

            var result = await _accountsWorkflow.GetAllEntriesSortedAsync();
            if (result.IsFailed)
            {
                _messageService.ShowResultError(result);
                return;
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(result.Value, options));

            var psi = new ProcessStartInfo { FileName = path, UseShellExecute = true };

            Process.Start(psi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export secrets to file failed.");
            _messageService.ShowError(UI.ex_UnexpectedError);
        }
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
        _clipboardService.CopyAndScheduleClear(TotpCode, TimeSpan.FromSeconds(30));
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
