using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TOTP.Core.Enums;
using TOTP.Infrastructure.Extensions;
using TOTP.Infrastructure.Parser;
using TOTP.Resources;
using TOTP.Validation;
using Application = System.Windows.Application;
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
            _messageService.ShowError(UI.ex_Adding_New_TOTP );
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
            _messageService.ShowError(UI.ex_UpdatingSecret );
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
            _messageService.ShowError(UI.ex_UpdatingSecret );
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
        _isDoubleClick = true;
        Debug.WriteLine("***** _isDoubleClick = true;  ***");

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
            if (Application.Current.MainWindow == null)
                return;

            var decodedQrCode = _qrScannerDialogFactory().ScanQrCode(Application.Current.MainWindow);

            if (!string.IsNullOrWhiteSpace(decodedQrCode))
            {
                OtpauthParser.TOTPData? otp = null;

                try
                {
                    otp = OtpauthParser.Parse(decodedQrCode);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, UI.msg_ErrorParsingOtpUrl);
                    _messageService.ShowError(UI.msg_ErrorParsingOtpUrl);
                    return;
                }

                var newAccountItem = new OtpViewModel(Guid.NewGuid(), otp.Issuer ?? string.Empty, otp.SecretBase32, otp.Label);
                var existing = FindMatchingEntry(newAccountItem, AllOtps);

                if (existing != null)
                {
                    if (SecretsEquivalent(existing.Secret, newAccountItem.Secret))
                    {
                        _messageService.ShowInfo("Account already exists. No changes applied.");
                        return;
                    }

                    var shouldUpdate = _messageService.ConfirmInfo(
                        $"Account '{existing.Issuer}' already exists. Update existing account with the scanned values?",
                        "Update existing",
                        "More options");

                    if (shouldUpdate)
                    {
                        var updated = newAccountItem.Copy();
                        if (updated == null)
                            return;

                        updated.ID = existing.ID;

                        var updateResult = await _accountsWorkflow.UpdateAsync(existing, updated);
                        if (updateResult.IsFailed)
                        {
                            _messageService.ShowResultError(updateResult, existing.Issuer);
                            return;
                        }

                        existing.UpdateSelf(updated);
                        _messageService.ShowSuccess("Account updated from scanned QR.", 2);

                        if (SelectedAccount?.ID == existing.ID && !ShowGenerateQrCodeLink)
                            UpdateQRCode();

                        return;
                    }

                    var keepBoth = _messageService.ConfirmInfo(
                        "Do you want to keep both entries?",
                        "Keep both",
                        UI.ui_btnCancel);

                    if (!keepBoth)
                        return;

                    newAccountItem.ID = Guid.NewGuid();
                    newAccountItem.Issuer = CreateKeepBothIssuer(newAccountItem.Issuer, AllOtps);
                }

                var validationErrors = _accountsWorkflow.ValidateForCreate(newAccountItem, AllOtps);
                if (validationErrors.Count > 0)
                {

                    foreach (var error in validationErrors)
                    {
                        if (error == ValidationError.PlatformAlreadyExists)
                        {
                            _messageService.ShowError(ValidationMessageMapper.ToMessage(error, newAccountItem.Issuer ?? string.Empty));
                        }
                        else
                            _messageService.ShowError(ValidationMessageMapper.ToMessage(error));
                    }

                    return;
                }

                try
                {
                    var result = await _accountsWorkflow.AddAsync(newAccountItem);
                    if (result.IsFailed)
                    {
                        _messageService.ShowResultError(result, newAccountItem.Issuer);
                        return;
                    }

                    AllOtps.Add(newAccountItem);

                }
                finally
                {
                    IsAddMode = false;
                    IsEditAddFlyoutOpen = false;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UI.ex_Adding_New_TOTP);
            _messageService.ShowError(UI.ex_Adding_New_TOTP );
        }
    }

    private static OtpViewModel? FindMatchingEntry(OtpViewModel incoming, System.Collections.Generic.IEnumerable<OtpViewModel> allOtps)
    {
        var byIssuerAndAccount = allOtps.FirstOrDefault(existing =>
            string.Equals(existing.Issuer ?? string.Empty, incoming.Issuer ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(existing.AccountName ?? string.Empty, incoming.AccountName ?? string.Empty, StringComparison.OrdinalIgnoreCase));
        if (byIssuerAndAccount != null)
            return byIssuerAndAccount;

        return allOtps.FirstOrDefault(existing =>
            string.Equals(existing.Issuer ?? string.Empty, incoming.Issuer ?? string.Empty, StringComparison.OrdinalIgnoreCase));
    }

    private static string CreateKeepBothIssuer(string? baseIssuer, System.Collections.Generic.IEnumerable<OtpViewModel> allOtps)
    {
        var source = string.IsNullOrWhiteSpace(baseIssuer) ? "Imported" : baseIssuer.Trim();
        var candidate = $"{source} (imported)";
        var suffix = 2;

        while (allOtps.Any(item => string.Equals(item.Issuer ?? string.Empty, candidate, StringComparison.OrdinalIgnoreCase)))
        {
            candidate = $"{source} (imported {suffix})";
            suffix++;
        }

        return candidate;
    }

    private static bool SecretsEquivalent(string? left, string? right)
    {
        static string Normalize(string? value) =>
            new string((value ?? string.Empty).Where(ch => !char.IsWhiteSpace(ch) && ch != '-').ToArray())
                .TrimEnd('=')
                .ToUpperInvariant();

        return string.Equals(Normalize(left), Normalize(right), StringComparison.Ordinal);
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
