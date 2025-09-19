using System.Collections.Generic;
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

    public UiValidation ValidateAll()
    {
        ValidatePlatform().ValidateSecret();
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

    //public UiValidation ValidateAccount()
    //{
    //    var error = SecretValidator.ValidateAccount(_item.Account);
    //    if (error != ValidationError.None)
    //        _errors.Add(error);
    //    return this;
    //}

    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<ValidationError> Errors => _errors;
}


