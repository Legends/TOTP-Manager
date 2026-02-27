using System;
using System.Collections.Generic;
using System.Linq;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.ViewModels;

namespace TOTP.Validation;


public record ValidationResult(bool IsValid, ValidationError Error);
internal class UiValidation
{

    private readonly OtpViewModel _item;
    private IEnumerable<OtpViewModel>? _source;
    private readonly List<ValidationError> _errors = new();

    public UiValidation(OtpViewModel item, IEnumerable<OtpViewModel>? source = null)
    {
        _item = item;
        _source = source;
    }

    public static UiValidation Use(OtpViewModel item, IEnumerable<OtpViewModel>? source = null)
    {
        return new UiValidation(item, source);
    }

    /// <summary>
    /// Validates all fields of the SecretItemViewModel except duplicates
    /// </summary>
    /// <returns></returns>
    public UiValidation ValidateAll()
    {
        ValidatePlatform().ValidateSecret().ValidateID();
        return this;
    }

    public UiValidation ValidatePlatform()
    {
        var error = ValidatePlatformName(_item.Issuer);
        if (error != ValidationError.None)
            _errors.Add(error);
        return this;
    }

    public UiValidation ValidateSecret()
    {
        var error = ValidateSecretValue(_item.Secret);
        if (error != ValidationError.None)
            _errors.Add(error);
        return this;
    }

    public UiValidation ValidateID()
    {
        var error = ValidateID(_item.ID);
        if (error != ValidationError.None)
            _errors.Add(error);
        return this;
    }

   
    /// <summary>
    /// Checks for account duplicates in source list.
    /// If source is not provided, it will use the one provided in the constructor.
    /// If that is also null, it will throw an exception.
    /// </summary>
    /// <param name="source">The source item list</param>
    /// <param name="excludeSelf"></param>
    /// <returns></returns>
    public UiValidation PlatformNameDuplicateExists(IEnumerable<OtpViewModel>? source = null, bool excludeSelf = false)
    {
        var src = source ?? _source;
        ArgumentNullException.ThrowIfNull(src, nameof(source));

        if (excludeSelf)
            src = src.Where(sivm => sivm.ID != _item.ID);

        // Check duplicates in the bound list (ignore the current row)
        bool duplicate = src
            .Any(x => string.Equals(x.Issuer, _item.Issuer, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
            _errors.Add(ValidationError.PlatformAlreadyExists);
        return this;
    }

    
    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<ValidationError> Errors => _errors;


    public static ValidationError ValidateID(Guid id)
    {
        return id == Guid.Empty ? ValidationError.IdRequired : ValidationError.None;
    }

    public static ValidationError ValidatePlatformName(string? input)
    {
        return string.IsNullOrWhiteSpace(input)
            ? ValidationError.PlatformRequired
            : ValidationError.None;
    }
    public static ValidationError PlatformNameDuplicateExists(string platform, IEnumerable<OtpEntry> source)
    {
        // Check duplicates in the bound list (ignore the current row)
        bool duplicate = source
            .Any(x => string.Equals(x.Issuer, platform, StringComparison.OrdinalIgnoreCase));
        return duplicate ? ValidationError.PlatformAlreadyExists : ValidationError.None;
    }
    public static ValidationError ValidateSecretValue(string? secretValue)
    {
        return string.IsNullOrWhiteSpace(secretValue)
            ? ValidationError.SecretRequired
            : IsValidBase32Format(secretValue)
                ? ValidationError.None
                : ValidationError.SecretInvalidFormat;
    }

    public static bool IsValidBase32Format(string secretValue)
    {
        try
        {
            var bytes = OtpNet.Base32Encoding.ToBytes(secretValue);
            return bytes.Length > 0;
        }
        catch
        {
            return false;
        }
    }
}


