using FluentResults;
using Moq;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Infrastructure.Services;

namespace TOTP.Tests.Infrastructure.Services;

public sealed class AccountTotpServiceTests
{
    [Fact]
    public async Task GenerateAsync_UsesMatchingAccountSeedBehindServiceBoundary()
    {
        var id = Guid.NewGuid();
        const string syntheticSeed = "JBSWY3DPEHPK3PXP";
        var manager = new Mock<IAccountManager>();
        manager.Setup(value => value.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(Result.Ok<IReadOnlyList<Account>>(
                [new Account(id, "Issuer", syntheticSeed, "account")]));
        var generator = new Mock<ITotpGenerator>();
        generator.Setup(value => value.Generate(syntheticSeed))
            .Returns(new TotpGenerationResult("123456", 15, 30));
        var sut = new AccountTotpService(manager.Object, generator.Object);

        var result = await sut.GenerateAsync(id);

        Assert.True(result.IsSuccess);
        Assert.Equal("123456", result.Value.Code);
        generator.Verify(value => value.Generate(syntheticSeed), Times.Once);
    }

    [Fact]
    public async Task GenerateAsync_WhenSeedIsInvalid_ReturnsGenericFailure()
    {
        var id = Guid.NewGuid();
        var manager = new Mock<IAccountManager>();
        manager.Setup(value => value.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(Result.Ok<IReadOnlyList<Account>>(
                [new Account(id, "Issuer", "INVALID-SECRET", "account")]));
        var generator = new Mock<ITotpGenerator>();
        generator.Setup(value => value.Generate(It.IsAny<string>()))
            .Throws(new FormatException("INVALID-SECRET"));
        var sut = new AccountTotpService(manager.Object, generator.Object);

        var result = await sut.GenerateAsync(id);

        Assert.True(result.IsFailed);
        Assert.DoesNotContain("INVALID-SECRET", result.Errors[0].Message, StringComparison.Ordinal);
    }
}
