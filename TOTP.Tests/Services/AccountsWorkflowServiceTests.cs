using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.ObjectModel;
using TOTP.Core.Common;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Services;
using TOTP.ViewModels;

namespace TOTP.Tests.Services;

public sealed class AccountsWorkflowServiceTests
{
    [Fact]
    public async Task LoadAllAsync_WhenManagerSucceeds_MapsToViewModels()
    {
        var manager = new Mock<IOtpManager>();
        manager.Setup(m => m.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(Result.Ok(new ObservableCollection<Account>
            {
                new(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP", "john")
            }));

        var sut = CreateSut(manager);

        var result = await sut.LoadAllAsync();

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("GitHub", result.Value[0].Issuer);
        Assert.Equal("john", result.Value[0].AccountName);
    }

    [Fact]
    public async Task LoadAllAsync_WhenManagerReturnsFailure_PropagatesFailure()
    {
        var manager = new Mock<IOtpManager>();
        manager.Setup(m => m.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(Result.Fail<ObservableCollection<Account>>("load failed"));

        var sut = CreateSut(manager);

        var result = await sut.LoadAllAsync();

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message.Contains("load failed", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task LoadAllAsync_WhenManagerThrows_ReturnsTokensLoadFailed()
    {
        var manager = new Mock<IOtpManager>();
        manager.Setup(m => m.GetAllOtpEntriesSortedAsync()).ThrowsAsync(new InvalidOperationException("boom"));

        var sut = CreateSut(manager);

        var result = await sut.LoadAllAsync();

        Assert.True(result.IsFailed);
        AssertAppError(result.Errors, AppErrorCode.AccountsLoadFailed);
    }

    [Fact]
    public async Task GetAllEntriesSortedAsync_WhenManagerThrows_ReturnsTokensLoadFailed()
    {
        var manager = new Mock<IOtpManager>();
        manager.Setup(m => m.GetAllOtpEntriesSortedAsync()).ThrowsAsync(new InvalidOperationException("boom"));

        var sut = CreateSut(manager);

        var result = await sut.GetAllEntriesSortedAsync();

        Assert.True(result.IsFailed);
        AssertAppError(result.Errors, AppErrorCode.AccountsLoadFailed);
    }

    [Fact]
    public async Task AddAsync_WhenManagerThrows_ReturnsTokensCreateFailed()
    {
        var manager = new Mock<IOtpManager>();
        manager.Setup(m => m.AddNewAsync(It.IsAny<Account>())).ThrowsAsync(new Exception("boom"));

        var sut = CreateSut(manager);

        var result = await sut.AddAsync(new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP", "john"));

        Assert.True(result.IsFailed);
        AssertAppError(result.Errors, AppErrorCode.AccountsCreateFailed);
    }

    [Fact]
    public async Task AddAsync_WhenManagerSucceeds_MapsAndForwardsEntry()
    {
        var manager = new Mock<IOtpManager>();
        Account? captured = null;
        manager.Setup(m => m.AddNewAsync(It.IsAny<Account>()))
            .Callback<Account>(e => captured = e)
            .ReturnsAsync(Result.Ok());

        var sut = CreateSut(manager);
        var vm = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP", "john");

        var result = await sut.AddAsync(vm);

        Assert.True(result.IsSuccess);
        Assert.NotNull(captured);
        Assert.Equal(vm.ID, captured!.ID);
        Assert.Equal(vm.Issuer, captured.Issuer);
    }

    [Fact]
    public async Task UpdateAsync_WithNullPrevious_ForwardsNullAndUpdated()
    {
        var manager = new Mock<IOtpManager>();
        Account? previousCaptured = new(Guid.NewGuid(), "seed", "seed", "seed");
        Account? updatedCaptured = null;

        manager.Setup(m => m.UpdateAsync(It.IsAny<Account>(), It.IsAny<Account>()))
            .Callback<Account, Account>((p, u) => { previousCaptured = p; updatedCaptured = u; })
            .ReturnsAsync(Result.Ok());

        var sut = CreateSut(manager);
        var updated = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP", "john");

        var result = await sut.UpdateAsync(null, updated);

        Assert.True(result.IsSuccess);
        Assert.Null(previousCaptured);
        Assert.NotNull(updatedCaptured);
        Assert.Equal(updated.ID, updatedCaptured!.ID);
    }

    [Fact]
    public async Task UpdateAsync_WhenManagerThrows_ReturnsTokensUpdateFailed()
    {
        var manager = new Mock<IOtpManager>();
        manager.Setup(m => m.UpdateAsync(It.IsAny<Account>(), It.IsAny<Account>())).ThrowsAsync(new Exception("boom"));

        var sut = CreateSut(manager);

        var result = await sut.UpdateAsync(
            new OtpViewModel(Guid.NewGuid(), "old", "JBSWY3DPEHPK3PXP", "john"),
            new OtpViewModel(Guid.NewGuid(), "new", "JBSWY3DPEHPK3PXP", "john"));

        Assert.True(result.IsFailed);
        AssertAppError(result.Errors, AppErrorCode.AccountsUpdateFailed);
    }

    [Fact]
    public async Task DeleteAsync_WhenManagerThrows_ReturnsTokensDeleteFailed()
    {
        var manager = new Mock<IOtpManager>();
        manager.Setup(m => m.DeleteAsync(It.IsAny<Account>())).ThrowsAsync(new Exception("boom"));

        var sut = CreateSut(manager);

        var result = await sut.DeleteAsync(new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP", "john"));

        Assert.True(result.IsFailed);
        AssertAppError(result.Errors, AppErrorCode.AccountsDeleteFailed);
    }

    [Fact]
    public void ValidateForCreate_WhenInvalidAndDuplicate_ReturnsAllExpectedErrors()
    {
        var sut = CreateSut(new Mock<IOtpManager>());
        var item = new OtpViewModel(Guid.Empty, "GitHub", "%%%", "john");
        var source = new[] { new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP", "other") };

        var errors = sut.ValidateForCreate(item, source);

        Assert.Contains(ValidationError.IdRequired, errors);
        Assert.Contains(ValidationError.SecretInvalidFormat, errors);
        Assert.Contains(ValidationError.PlatformAlreadyExists, errors);
    }

    [Fact]
    public void ValidateForUpdate_ExcludesSelfButFlagsOtherDuplicate()
    {
        var sut = CreateSut(new Mock<IOtpManager>());
        var id = Guid.NewGuid();
        var item = new OtpViewModel(id, "GitHub", "JBSWY3DPEHPK3PXP", "john");

        var onlySelf = new[] { new OtpViewModel(id, "GitHub", "JBSWY3DPEHPK3PXP", "john") };
        var selfPlusOther = new[]
        {
            new OtpViewModel(id, "GitHub", "JBSWY3DPEHPK3PXP", "john"),
            new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP", "jane")
        };

        var errorsSelfOnly = sut.ValidateForUpdate(item, onlySelf);
        var errorsWithOther = sut.ValidateForUpdate(item, selfPlusOther);

        Assert.DoesNotContain(ValidationError.PlatformAlreadyExists, errorsSelfOnly);
        Assert.Contains(ValidationError.PlatformAlreadyExists, errorsWithOther);
    }

    [Fact]
    public void CheckDuplicateIssuer_UsesIdEqualityToExcludeCurrent()
    {
        var sut = CreateSut(new Mock<IOtpManager>());
        var id = Guid.NewGuid();
        var current = new OtpViewModel(id, "GitHub", "AAAA", "john");

        var sourceNoOtherDuplicate = new[]
        {
            new OtpViewModel(id, "GitHub", "BBBB", "john")
        };

        var sourceWithOtherDuplicate = new[]
        {
            new OtpViewModel(id, "GitHub", "BBBB", "john"),
            new OtpViewModel(Guid.NewGuid(), "GitHub", "CCCC", "jane")
        };

        var noDuplicate = sut.CheckDuplicateIssuer(current, sourceNoOtherDuplicate);
        var duplicate = sut.CheckDuplicateIssuer(current, sourceWithOtherDuplicate);

        Assert.Equal(ValidationError.None, noDuplicate);
        Assert.Equal(ValidationError.PlatformAlreadyExists, duplicate);
    }

    private static AccountsWorkflowService CreateSut(Mock<IOtpManager> manager)
        => new(manager.Object, Mock.Of<ILogger<AccountsWorkflowService>>());

    private static void AssertAppError(IReadOnlyList<IError> errors, AppErrorCode expectedCode)
    {
        var appError = Assert.IsType<AppError>(errors.First());
        Assert.Equal(expectedCode, appError.Code);
    }
}
