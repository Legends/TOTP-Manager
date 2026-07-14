using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TOTP.Core.Enums;
using TOTP.Presentation.Extensions;
using TOTP.Resources;
using TOTP.Services.Interfaces;
using TOTP.Validation;
using ValidationError = TOTP.Core.Enums.ValidationError;

namespace TOTP.ViewModels;

public partial class MainViewModel
{
    #region ### OPEN FLYOUT IN ADD MODE ###

    public void OpenFlyoutAddMode()
    {
        try
        {
            //throw new Exception("dummy");
            ClearEditingSecretCache();
            IsAddMode = true;
            IsEditAddFlyoutOpen = true;
            CurrentSecretBeingEditedOrAdded = new OtpViewModel(Guid.NewGuid(), string.Empty, string.Empty, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_Adding_New_TOTP);
            _messageService.ShowError(UI.ex_Adding_New_TOTP);
        }
    }

    #endregion

    #region ### OPEN FLYOUT IN EDIT MODE ###

    public void OpenFlyoutEditMode(OtpViewModel? item)
    {
        try
        {
            if (item == null) return;

            ClearEditingSecretCache();
            IsAddMode = false;
            CurrentSecretBeingEditedOrAdded = item.Copy();
            IsEditAddFlyoutOpen = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_UpdatingSecret);
            _messageService.ShowError(UI.ex_UpdatingSecret);
        }
    }

    #endregion

    #region ### CLOSE/CANCEL FLYOUT ###

    void CancelFlyout()
    {
        ClearEditingSecretCache();
        IsEditAddFlyoutOpen = false;
        IsAddMode = false;
        IsSecretVisible = false;
    }

    #endregion

    #region ### DELETE OTP ENTRY ###

    internal async Task DeleteAccountAsync(OtpViewModel? item)
    {
        try
        {
            if (item == null)
                return;

            var shouldDelete = _messageService.ConfirmWarning(string.Format(UI.msg_ConfirmDeleteSecret, item.Issuer), UI.ui_btnDelete);
            if (!shouldDelete)
                return;

            var result = await _accountsWorkflow.DeleteAsync(item);

            if (result.IsFailed)
            {
                _messageService.ShowResultError(result, item.Issuer);
                return;
            }

            AllOtps.Remove(item);
            OnPropertyChanged(nameof(AllOtps));
            if (item.ID == SelectedAccount?.ID)
            {
                SelectedAccount = null; // we set it to null if the SelectedAccount was deleted before
                StopTotpTimer();
                ClearCodeGenerationOutput();
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_DeletingSecret);
            _messageService.ShowError(string.Format(UI.ex_DeletingSecret_0, ex.Message));
        }
    }

    #endregion

    #region ### UPDATE OTP ENTRY ###

    public async Task UpdateAccountAsync(OtpViewModel updated)
    {
        try
        {
            var result = await _accountsWorkflow.UpdateAsync(PreviousVersion, updated);

            if (result.IsFailed)
            {
                _messageService.ShowResultError(result, updated.Issuer ?? string.Empty);
                return;
            }

            var itemToBeUpdated = AllOtps.FirstOrDefault(s => s.ID == updated.ID);

            itemToBeUpdated?.UpdateSelf(updated);
            OnPropertyChanged(nameof(AllOtps));

            if (updated.ID == SelectedAccount?.ID && !ShowGenerateQrCodeLink)
                UpdateQRCode();

            PreviousVersion = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_UpdatingSecret);
            _messageService.ShowError(string.Format(UI.ex_UpdatingSecret_0, ex.Message));
        }
    }

    private void UpdateQRCode()
    {
        if (SelectedAccount == null)
            return;

        QrCodeImage = GenerateQRCodeImage(SelectedAccount);
    }

    public async Task AddOrUpdateAccountAsync()
    {
        try
        {
            IsSecretVisible = false;

            if (IsAddMode)
            {
                if (CurrentSecretBeingEditedOrAdded == null)
                    return;

                var current = CurrentSecretBeingEditedOrAdded;
                current.SetDuplicateCheck(DuplicateCheck);
                var validationErrors = _accountsWorkflow.ValidateForCreate(current, AllOtps);
                if (validationErrors.Count > 0)
                {
                    current.RefreshValidation();
                    return;
                }

                var result = await _accountsWorkflow.AddAsync(current);

                if (result.IsFailed)
                {
                    _messageService.ShowResultError(result, current.Issuer);
                    return;
                }

                var itemToAdd = current.Copy();
                if (itemToAdd == null)
                    return;

                AllOtps.Add(itemToAdd);

                ClearEditingSecretCache();
                IsAddMode = false;
                IsEditAddFlyoutOpen = false;
            }
            else
            {
                if (CurrentSecretBeingEditedOrAdded == null)
                    return;

                var updated = CurrentSecretBeingEditedOrAdded.Copy();

                if (updated == null)
                    return;

                var validationErrors = _accountsWorkflow.ValidateForUpdate(updated, AllOtps);
                if (validationErrors.Count > 0)
                {
                    CurrentSecretBeingEditedOrAdded.RefreshValidation();
                    return;
                }

                await UpdateAccountAsync(CurrentSecretBeingEditedOrAdded);
                ClearEditingSecretCache();
                IsEditAddFlyoutOpen = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_UpdatingSecret);
            _messageService.ShowError(UI.ex_UpdatingSecret);
        }
    }

    #region ### INLINE EDITING LOGIC FOR SYNCFUSION DATAGRID ###

    private void OnBeginEdit(OtpViewModel item)
    {
        PreviousVersion = item.Copy();
        item.IsBeingEdited = true;
        IsInlineEditing = true;
    }

    public bool IsInlineEditing { get; set; }

    private async Task OnEndEditAsync(OtpViewModel? item)
    {
        try
        {
            if (item == null || PreviousVersion == null)
                return;

            if (item.ID != PreviousVersion.ID)
                return;

            item.IsBeingEdited = false;

            if (!OtpViewModelValueComparer.Default.Equals(item, PreviousVersion))
            {
                var validation = UiValidation.Use(item).ValidateAll();

                if (!validation.IsValid)
                {
                    _messageService.ShowInfo(ValidationMessageMapper.ToMessage(validation.Errors.FirstOrDefault()));
                    return;
                }

                await UpdateAccountAsync(item);
            }

            PreviousVersion = null;
            IsInlineEditing = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_UpdatingSecret);
            _messageService.ShowError(UI.ex_UpdatingSecret);
        }
    }

    private void OnDoubleClick(OtpViewModel item)
    {
        foreach (var s in AllOtps)
            s.IsBeingEdited = false;

        item.IsBeingEdited = !item.IsBeingEdited;
        Debug.WriteLine("OnDoubleClick");
    }

    #endregion

    #endregion

    #region ### QR Code - Create - Scan - Add ###

    public async Task ScanQrAndAddAccountAsync()
    {
        try
        {
            var scannerSw = Stopwatch.StartNew();
            var openAttempt = Interlocked.Increment(ref _scannerOpenCount);
            _logger.LogInformation("scanner.open.begin attempt={Attempt} first_open={FirstOpen}", openAttempt, openAttempt == 1);
            var decodedQrCode = _qrScannerDialogService.ScanQrCode();
            _logger.LogInformation("scanner.open.end attempt={Attempt} first_open={FirstOpen} elapsed_ms={ElapsedMs} decoded={Decoded}",
                openAttempt, openAttempt == 1, scannerSw.ElapsedMilliseconds, !string.IsNullOrWhiteSpace(decodedQrCode));

            if (string.IsNullOrWhiteSpace(decodedQrCode))
            {
                return;
            }

            try
            {
                var result = await _qrAccountImportWorkflow.ImportAsync(decodedQrCode, AllOtps);
                if (result.ChangeKind == QrAccountImportChangeKind.Updated &&
                    result.AccountId == SelectedAccount?.ID &&
                    !ShowGenerateQrCodeLink)
                {
                    UpdateQRCode();
                }
            }
            finally
            {
                IsAddMode = false;
                IsEditAddFlyoutOpen = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_Adding_New_TOTP);
            _messageService.ShowError(UI.ex_Adding_New_TOTP);
        }
    }

    #endregion

    private void ClearEditingSecretCache()
    {
        if (CurrentSecretBeingEditedOrAdded != null)
        {
            CurrentSecretBeingEditedOrAdded.ClearSensitiveData();
            CurrentSecretBeingEditedOrAdded = null;
        }
    }
}
