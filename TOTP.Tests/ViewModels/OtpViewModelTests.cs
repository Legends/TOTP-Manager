using TOTP.Core.Enums;
using TOTP.Resources;
using TOTP.Validation;
using TOTP.ViewModels;

namespace TOTP.Tests.ViewModels;

public sealed class OtpViewModelTests
{
    [Fact]
    public void Indexer_IssuerEmpty_ReturnsPlatformRequiredMessage()
    {
        var vm = new OtpViewModel(Guid.NewGuid(), "", "JBSWY3DPEHPK3PXP", "john");

        var error = vm[nameof(vm.Issuer)];

        Assert.Equal(UI.msg_PlatformRequired, error);
    }

    [Fact]
    public void Indexer_IssuerDuplicateCheckEnabled_AppendsDuplicateMessage()
    {
        var vm = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP", "john");
        vm.SetDuplicateCheck(_ => ValidationError.PlatformAlreadyExists);

        var error = vm[nameof(vm.Issuer)];

        Assert.Equal(string.Format(UI.msg_Platform_Exists, "GitHub"), error);
    }

    [Fact]
    public void Indexer_SecretInvalid_ReturnsSecretInvalidMessage()
    {
        var vm = new OtpViewModel(Guid.NewGuid(), "GitHub", "%%%", "john");

        var error = vm[nameof(vm.Secret)];

        Assert.Equal(UI.msg_SecretInvalidFormat, error);
    }

    [Fact]
    public void Indexer_SecretValid_ReturnsEmpty()
    {
        var vm = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP", "john");

        var error = vm[nameof(vm.Secret)];

        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public void RefreshValidation_PopulatesInlineErrorProperties()
    {
        var vm = new OtpViewModel(Guid.Empty, "", "%%%", "john");
        vm.SetDuplicateCheck(_ => ValidationError.PlatformAlreadyExists);

        vm.RefreshValidation();

        Assert.Contains(UI.msg_PlatformRequired, vm.PlatformError);
        Assert.Contains(string.Format(UI.msg_Platform_Exists, ""), vm.PlatformError);
        Assert.Equal(UI.msg_SecretInvalidFormat, vm.SecretError);
        Assert.Equal(string.Empty, vm.TokenError);
    }

    [Fact]
    public void BeginEditAndCancelEdit_RestoresOriginalValues()
    {
        var vm = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP", "john");

        vm.BeginEdit();
        vm.Issuer = "Azure";
        vm.Secret = "%%%";
        vm.TokenName = "other";

        vm.CancelEdit();

        Assert.Equal("GitHub", vm.Issuer);
        Assert.Equal("JBSWY3DPEHPK3PXP", vm.Secret);
        Assert.Equal("john", vm.TokenName);
    }

    [Fact]
    public void EndEdit_ClearsBackupAndHighlights()
    {
        var vm = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP", "john");

        vm.BeginEdit();
        vm.EndEdit();

        Assert.True(vm.IsHighlighted);
    }

    [Fact]
    public void Copy_CreatesShallowCloneWithSameValues()
    {
        var vm = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP", "john");

        var copy = vm.Copy();

        Assert.NotNull(copy);
        Assert.NotSame(vm, copy);
        Assert.Equal(vm.ID, copy!.ID);
        Assert.Equal(vm.Issuer, copy.Issuer);
        Assert.Equal(vm.Secret, copy.Secret);
        Assert.Equal(vm.TokenName, copy.TokenName);
    }

    [Fact]
    public void UpdateSelf_OverwritesIssuerSecretAndToken()
    {
        var vm = new OtpViewModel(Guid.NewGuid(), "GitHub", "OLD", "john");
        var changed = new OtpViewModel(vm.ID, "Azure", "NEW", "jane");

        vm.UpdateSelf(changed);

        Assert.Equal("Azure", vm.Issuer);
        Assert.Equal("NEW", vm.Secret);
        Assert.Equal("jane", vm.TokenName);
    }

    [Fact]
    public void Equals_UsesIdOnly()
    {
        var id = Guid.NewGuid();
        var left = new OtpViewModel(id, "A", "AAA", "x");
        var right = new OtpViewModel(id, "B", "BBB", "y");

        Assert.True(left.Equals(right));
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }
}

public sealed class OtpViewModelValueComparerTests
{
    [Fact]
    public void Equals_IgnoresCaseWhitespaceAndSecretFormatting()
    {
        var left = new OtpViewModel(Guid.NewGuid(), " GitHub ", "JBSW Y3DP-EHPK3PXP===", " John@Doe.com ");
        var right = new OtpViewModel(Guid.NewGuid(), "github", "jbswy3dpehpk3pxp", "john@doe.com");

        var equal = OtpViewModelValueComparer.Default.Equals(left, right);

        Assert.True(equal);
    }

    [Fact]
    public void GetHashCode_NormalizesComparableFields()
    {
        var left = new OtpViewModel(Guid.NewGuid(), " GitHub ", "JBSW Y3DP-EHPK3PXP===", " John@Doe.com ");
        var right = new OtpViewModel(Guid.NewGuid(), "github", "jbswy3dpehpk3pxp", "john@doe.com");

        var h1 = OtpViewModelValueComparer.Default.GetHashCode(left);
        var h2 = OtpViewModelValueComparer.Default.GetHashCode(right);

        Assert.Equal(h1, h2);
    }

    [Fact]
    public void Equals_WhenSecretDiffers_ReturnsFalse()
    {
        var left = new OtpViewModel(Guid.NewGuid(), "GitHub", "AAAA", "john");
        var right = new OtpViewModel(Guid.NewGuid(), "GitHub", "BBBB", "john");

        Assert.False(OtpViewModelValueComparer.Default.Equals(left, right));
    }
}
