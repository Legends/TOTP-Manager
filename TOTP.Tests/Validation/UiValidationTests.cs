using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Validation;
using TOTP.ViewModels;

namespace TOTP.Tests.Validation;

public sealed class UiValidationTests
{
    [Fact]
    public void ValidatePlatformName_WhenNullOrWhitespace_ReturnsPlatformRequired()
    {
        Assert.Equal(ValidationError.PlatformRequired, UiValidation.ValidatePlatformName(null));
        Assert.Equal(ValidationError.PlatformRequired, UiValidation.ValidatePlatformName("   "));
        Assert.Equal(ValidationError.None, UiValidation.ValidatePlatformName("GitHub"));
    }

    [Fact]
    public void ValidateID_WhenEmpty_ReturnsIdRequired()
    {
        Assert.Equal(ValidationError.IdRequired, UiValidation.ValidateID(Guid.Empty));
        Assert.Equal(ValidationError.None, UiValidation.ValidateID(Guid.NewGuid()));
    }

    [Fact]
    public void ValidateSecretValue_CoversRequiredInvalidAndValid()
    {
        Assert.Equal(ValidationError.SecretRequired, UiValidation.ValidateSecretValue(null));
        Assert.Equal(ValidationError.SecretRequired, UiValidation.ValidateSecretValue("  "));
        Assert.Equal(ValidationError.SecretInvalidFormat, UiValidation.ValidateSecretValue("%%%"));
        Assert.Equal(ValidationError.None, UiValidation.ValidateSecretValue("JBSWY3DPEHPK3PXP"));
    }

    [Fact]
    public void PlatformNameDuplicateExists_Static_IsCaseInsensitive()
    {
        var source = new[]
        {
            new Account(Guid.NewGuid(), "GitHub", "AAAA", "john")
        };

        var result = UiValidation.PlatformNameDuplicateExists("github", source);

        Assert.Equal(ValidationError.PlatformAlreadyExists, result);
    }

    [Fact]
    public void PlatformNameDuplicateExists_Instance_ThrowsWhenSourceMissing()
    {
        var item = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP", "john");

        var ex = Assert.Throws<ArgumentNullException>(() => UiValidation.Use(item).PlatformNameDuplicateExists());

        Assert.Equal("source", ex.ParamName);
    }

    [Fact]
    public void PlatformNameDuplicateExists_Instance_ExcludeSelfAvoidsFalsePositive()
    {
        var id = Guid.NewGuid();
        var item = new OtpViewModel(id, "GitHub", "JBSWY3DPEHPK3PXP", "john");
        var source = new[]
        {
            new OtpViewModel(id, "GitHub", "AAAA", "john")
        };

        var validator = UiValidation.Use(item, source).PlatformNameDuplicateExists(excludeSelf: true);

        Assert.True(validator.IsValid);
        Assert.DoesNotContain(ValidationError.PlatformAlreadyExists, validator.Errors);
    }

    [Fact]
    public void ValidateAll_WhenInvalidCollectsAllErrors()
    {
        var item = new OtpViewModel(Guid.Empty, "", "%%%", "john");

        var validator = UiValidation.Use(item).ValidateAll();

        Assert.False(validator.IsValid);
        Assert.Contains(ValidationError.PlatformRequired, validator.Errors);
        Assert.Contains(ValidationError.SecretInvalidFormat, validator.Errors);
        Assert.Contains(ValidationError.IdRequired, validator.Errors);
    }

    [Fact]
    public void IsValidBase32Format_ReturnsFalseForMalformed()
    {
        Assert.False(UiValidation.IsValidBase32Format("@notbase32@"));
        Assert.True(UiValidation.IsValidBase32Format("JBSWY3DPEHPK3PXP"));
    }

    [Fact]
    public void IsValidBase32Format_RejectsCharactersOutsideBase32Alphabet()
    {
        Assert.False(UiValidation.IsValidBase32Format("ABCD0EFG"));
        Assert.False(UiValidation.IsValidBase32Format("ABCD1EFG"));
        Assert.False(UiValidation.IsValidBase32Format("ABCD8EFG"));
        Assert.False(UiValidation.IsValidBase32Format("ABCD9EFG"));
    }

    [Fact]
    public void IsValidBase32Format_AllowsCommonSeparatorsAndPadding_WhenNormalized()
    {
        Assert.True(UiValidation.IsValidBase32Format(" jbsw-y3dp ehpk3pxp== "));
    }

    [Fact]
    public void IsValidBase32Format_RejectsTooShortSecrets()
    {
        Assert.False(UiValidation.IsValidBase32Format("JBSWY3DP"));
    }
}
