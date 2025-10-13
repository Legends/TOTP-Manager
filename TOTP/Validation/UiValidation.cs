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

    private readonly SecretItemViewModel _item;
    private readonly List<ValidationError> _errors = new();

    public UiValidation(SecretItemViewModel item)
    {
        _item = item;
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
        var error = SecretValidator.ValidateSecret(_item.Secret);
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

    public UiValidation PlatformNameDuplicateExists(string platform, IEnumerable<SecretItemViewModel> source)
    {
        // Check duplicates in the bound list (ignore the current row)
        bool duplicate = source
            .Any(x => string.Equals(x.Platform, platform, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
            _errors.Add(ValidationError.PlatformAlreadyExists);
        return this;
    }


    public UiValidation PlatformNameDuplicateExists(IEnumerable<SecretItemViewModel> source)
    {
        // Check duplicates in the bound list (ignore the current row)
        bool duplicate = source
            .Any(x => string.Equals(x.Platform, _item.Platform, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
            _errors.Add(ValidationError.PlatformAlreadyExists);
        return this;
    }


    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<ValidationError> Errors => _errors;
}


