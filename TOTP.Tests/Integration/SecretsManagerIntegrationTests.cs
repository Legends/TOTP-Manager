using Microsoft.Extensions.Logging;
using Moq.AutoMock;
using TOTP.Extensions;
using TOTP.Services;
using TOTP.ViewModels;

namespace TOTP.Tests.Integration;

public class SecretsManagerIntegrationTests : IDisposable
{
    private readonly string _testPath;
    private readonly AutoMocker _mocker;
    private readonly SecretsManager _secretsManager;

    private readonly SecretItemViewModel _initial = new("GitHub", "JBSWY3DPEHPK3PXP");
    private readonly SecretItemViewModel _updated = new("GitHub", "MZXW6YTBOI======");

    public SecretsManagerIntegrationTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), $"secrets-test-{Guid.NewGuid()}.dat");
        _mocker = new AutoMocker();

        _secretsManager = new SecretsManager(
            _mocker.Get<ILogger<SecretsManager>>(),
            _testPath
        );
    }

    [Fact]
    public async Task FullLifecycle_AddUpdateDelete_ShouldSucceed()
    {
        // --- ADD ---
        var resultAdd = await _secretsManager.AddNewItemAsync(_initial.ToDomain());
        Assert.True(resultAdd.Value);

        var resultAllSecrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Single(resultAllSecrets.Value);
        Assert.Equal(_initial.Platform, resultAllSecrets.Value[0].Platform);

        // --- UPDATE ---
        var resultUpdated = await _secretsManager.UpdateItemAsync(_initial.Platform, _updated.ToDomain());
        Assert.True(resultUpdated.Value);

        var updatedSecrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Single(updatedSecrets.Value);
        Assert.Equal(_updated.Secret, updatedSecrets.Value[0].Secret);

        // --- DELETE ---
        var deletedResult = await _secretsManager.DeleteItemAsync(_updated.Platform);
        Assert.True(deletedResult.Value);

        var resultAfterDelete = await _secretsManager.GetAllSecretsAsync();
        Assert.Empty(resultAfterDelete.Value);
    }

    [Fact]
    public async Task AddNewItem_ShouldPersistSecret()
    {
        var resultAdded = await _secretsManager.AddNewItemAsync(_initial.ToDomain());
        Assert.True(resultAdded.Value);

        var secrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Single(secrets.Value);
        Assert.Equal(_initial.Platform, secrets.Value[0].Platform);
    }

    [Fact]
    public async Task UpdateItem_ShouldReplaceSecret()
    {
        await _secretsManager.AddNewItemAsync(_initial.ToDomain());

        var updated = await _secretsManager.UpdateItemAsync(_initial.Platform, _updated.ToDomain());
        Assert.True(updated.Value);

        var secrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Single(secrets.Value);
        Assert.Equal(_updated.Secret, secrets.Value[0].Secret);
    }

    [Fact]
    public async Task DeleteItem_ShouldRemoveSecret()
    {
        await _secretsManager.AddNewItemAsync(_initial.ToDomain());
        await _secretsManager.UpdateItemAsync(_initial.Platform, _updated.ToDomain());

        var deleted = await _secretsManager.DeleteItemAsync(_updated.Platform);
        Assert.True(deleted.Value);

        var secrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Empty(secrets.Value);
    }


    [Fact]
    public async Task BackupSecretsFile_ShouldCreateBackup()
    {
        _ = await _secretsManager.AddNewItemAsync(_initial.ToDomain());

        var backupSuccess = _secretsManager.BackupSecretsFile();
        Assert.True(backupSuccess);

        var backupFile = _testPath + ".bak1";
        Assert.True(File.Exists(backupFile));
    }

    public void Dispose()
    {
        // Clean up all test and backup files
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
