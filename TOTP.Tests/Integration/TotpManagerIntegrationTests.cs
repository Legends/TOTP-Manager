using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using TOTP.Core.Enums;
using TOTP.Core.Events;
using TOTP.Core.Models;
using TOTP.Extensions;
using TOTP.Interfaces;
using TOTP.Services;
using TOTP.ViewModels;

namespace TOTP.Tests.Integration;

public class TotpManagerIntegrationTests : IDisposable
{
    private readonly string _testPath;
    private readonly AutoMocker _mocker;
    private readonly SecretsManager _secretsManager;
    private readonly TotpManager _totpManager;
    private readonly IMainViewModel _vm;

    private readonly SecretItemViewModel _initialSecret = new("GitHub", "JBSWY3DPEHPK3PXP");

    // we mock only what is necessary, like prompts and dialogs
    public TotpManagerIntegrationTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), $"secrets-test-{Guid.NewGuid()}.dat");

        _mocker = new AutoMocker();

        // Set up mocks
        _mocker.GetMock<IPlatformSecretDialogService>()
            .Setup(x => x.ShowForm(It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns((true, _initialSecret.Platform, _initialSecret.Secret));

        _mocker.GetMock<IMessageService>()
            .Setup(x => x.ShowMessageDialog(
                It.IsAny<string>(),
                It.IsAny<CaptionType>(),
                It.IsAny<string>()
               ))
            .Returns(true);

        _mocker.GetMock<IMessageService>()
            .Setup(x => x.ShowWarningMessageDialog(It.IsAny<string>()))
            .Returns(true);

        // Real SecretsManager with test file
        _secretsManager = new SecretsManager(_mocker.Get<ILogger<SecretsManager>>(), _testPath);
        _mocker.Use<ISecretsManager>(_secretsManager);

        _mocker.GetMock<IPlatformSecretDialogService>().Setup(ips => ips.ShowForm()).Returns((true, _initialSecret.Platform, _initialSecret.Secret));

        // Create real TotpManager
        _totpManager = _mocker.CreateInstance<TotpManager>();
        _vm = _mocker.CreateInstance<MainViewModel>();
    }


    /// <summary>
    /// We try to add the same secret twice => duplicate
    /// The 1st time it should succeed
    /// The 2nd time it should raise AlreadyExists once, then we cancel the operation
    /// </summary>
    /// <returns></returns>
    /// <exception cref="TimeoutException"></exception>
    [Fact]
    public async Task AddNewSecretAsync_RaisesAlreadyExists_ThenCancelsAndReturnsFalse()
    {
        var statuses = new List<OperationStatus>();

        var seenAlreadyExists = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        int promptCalls = 0;

        _totpManager.OnAddNewPrompt += _ =>
        {
            promptCalls++;

            // 1st call: Success=true  -> add succeeds
            // 2nd call (first loop iter): Success=true with same secret -> AlreadyExists
            // 3rd call (second loop iter): Success=false -> exit loop
            bool success = promptCalls != 3;

            return new AddNewPromptArgs
            {
                Success = success,
                Platform = _initialSecret.Platform,
                Secret = _initialSecret.Secret
            };
        };

        // Called only on AlreadyExists, LoadingFailed, StorageFailed
        // here it shoud be called only once with AlreadyExists
        _totpManager.OnMessageSend += (_, status, __) =>
        {
            statuses.Add(status);
            if (status == OperationStatus.AlreadyExists)
                seenAlreadyExists.TrySetResult();
        };

        // 1) First add succeeds
        var firstAddNewSecretAsyncTask = await _totpManager.AddNewSecretAsync();
        Assert.True(firstAddNewSecretAsyncTask.isSuccess);

        // 2) Second add: duplicate -> AlreadyExists once, then cancel to exit
        // this hits AlreadyExists on first run and
        // on second run AddNewPromptArgs returns false and quits the loop
        var secondAddNewSecretAsyncTask = _totpManager.AddNewSecretAsync();

        // Don't hang if event never comes
        var completed = await Task.WhenAny(seenAlreadyExists.Task, Task.Delay(2000));
        if (completed != seenAlreadyExists.Task)
            throw new TimeoutException($"Expected AlreadyExists; saw: [{string.Join(", ", statuses)}]");

        // Finish the method (third prompt returns Success=false)
        var second = await secondAddNewSecretAsyncTask.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.False(second.isSuccess);

        Assert.Contains(OperationStatus.AlreadyExists, statuses);  // from second call first iteration
    }


    [Fact]
    public async Task UpdateSecretAsync_ShouldReturnAlreadyExistsOnDuplicateSecret()
    {
        var existent = new SecretItem(_testPath, "test");
        var previous = new SecretItem("A", "AAAAAAAAA");
        var updated = new SecretItem(_testPath, "AAAAAAAAA");

        var source = new List<SecretItem>
        {
            existent,
            previous,
        };

        var result = await _totpManager.UpdateSecretAsync(previous, updated, source);
        Assert.False(result);

    }

    [Fact]
    public async Task Add_Compute_Update_Delete_Secret_ShouldSucceed()
    {
        //// --- ADD ---

        _totpManager.OnAddNewPrompt += (sender) =>
        {
            return new AddNewPromptArgs
            {
                Success = true,
                Platform = _initialSecret.Platform,
                Secret = _initialSecret.Secret
            };
        };

        var (success, item) = await _totpManager.AddNewSecretAsync();
        Assert.True(success);
        Assert.NotNull(item);

        var secrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Single(secrets.Value);
        Assert.Equal(_initialSecret.Platform, secrets.Value[0].Platform);

        // --- COMPUTE ---
        var codeResult = _totpManager.TryComputeCode(_initialSecret.Secret, out var code, out var error);
        Assert.True(codeResult);
        Assert.Matches(@"^\d{6}$", code!);
        Assert.Null(error);

        // --- UPDATE ---
        var updated = new SecretItemViewModel(_initialSecret.Platform, "MZXW6YTBOI======");
        await _totpManager.UpdateSecretAsync(_initialSecret.ToDomain(), updated.ToDomain(), secrets.Value);

        // --- FETCH ---
        var updatedSecrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Single(updatedSecrets.Value);
        Assert.Equal("MZXW6YTBOI======", updatedSecrets.Value[0].Secret);

        // --- DELETE ---
        _totpManager.ConfirmDeleteRequested += (arg1, arg2) => true;
        var deleteResult = await _totpManager.DeleteSecretAsync(updated.ToDomain());
        Assert.True(deleteResult);
        var result = await _secretsManager.GetAllSecretsAsync();
        Assert.Empty(result.Value);


    }


    public void Dispose()
    {
        if (File.Exists(_testPath))
            File.Delete(_testPath);

        for (int i = 1; i <= 5; i++)
        {
            var backup = _testPath + $".bak{i}";
            if (File.Exists(backup))
                File.Delete(backup);
        }
    }
}
