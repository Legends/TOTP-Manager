using System;
using System.Collections.Generic;
using System.Linq;
using TOTP.Core.Enums;
using TOTP.Core.Validation;
using TOTP.ViewModels;

namespace TOTP.Validation;


public record ValidationResult(bool IsValid, ValidationError Error);
internal class UiValidation
{

    private readonly AccountViewModel _item;
    private IEnumerable<AccountViewModel>? _source;
    private readonly List<ValidationError> _errors = new();

    public UiValidation(AccountViewModel item, IEnumerable<AccountViewModel>? source = null)
    {
        _item = item;
        _source = source;
    }

    public static UiValidation Use(AccountViewModel item, IEnumerable<AccountViewModel>? source = null)
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
        var error = SecretValidator.ValidatePlatform(_item.Platform);
        if (error != ValidationError.None)
            _errors.Add(error);
        return this;
    }

    public UiValidation ValidateSecret()
    {
        var error = SecretValidator.ValidateSecretValue(_item.Secret);
        if (error != ValidationError.None)
            _errors.Add(error);
        return this;
    }

    public UiValidation ValidateID()
    {
        var error = SecretValidator.ValidateID(_item.ID);
        if (error != ValidationError.None)
            _errors.Add(error);
        return this;
    }

    //public UiValidation PlatformNameDuplicateExists(string platform, IEnumerable<AccountViewModel> source)
    //{
    //    // Check duplicates in the bound list (ignore the current row)
    //    bool duplicate = source
    //        .Any(x => string.Equals(x.Platform, platform, StringComparison.OrdinalIgnoreCase));

    //    if (duplicate)
    //        _errors.Add(ValidationError.PlatformAlreadyExists);
    //    return this;
    //}


    /// <summary>
    /// Checks for account duplicates in source list.
    /// If source is not provided, it will use the one provided in the constructor.
    /// If that is also null, it will throw an exception.
    /// </summary>
    /// <param name="source">The source item list</param>
    /// <param name="excludeSelf"></param>
    /// <returns></returns>
    public UiValidation PlatformNameDuplicateExists(IEnumerable<AccountViewModel>? source = null, bool excludeSelf = false)
    {
        var src = source ?? _source;
        ArgumentNullException.ThrowIfNull(src, nameof(source));

        if (excludeSelf)
            src = src.Where(sivm => sivm.ID != _item.ID);

        // Check duplicates in the bound list (ignore the current row)
        bool duplicate = src
            .Any(x => string.Equals(x.Platform, _item.Platform, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
            _errors.Add(ValidationError.PlatformAlreadyExists);
        return this;
    }

    
    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<ValidationError> Errors => _errors;
}


