using Microsoft.Extensions.Logging;
using Moq.AutoMock;
using TOTP.Core.Services;
using TOTP.Core.Models;

namespace TOTP.Tests.Integration;

public class SecretsManagerIntegrationTests : IDisposable
{
    private readonly string _testPath;
    private readonly AutoMocker _mocker;
    private readonly SecretsManager _secretsManager;

    private readonly SecretItem _initial = new("GitHub", "JBSWY3DPEHPK3PXP");
    private readonly SecretItem _updated = new("GitHub", "MZXW6YTBOI======");

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
        var resultAdd = await _secretsManager.AddNewItemAsync(_initial);
        Assert.True(resultAdd.value);

        var resultAllSecrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Single(resultAllSecrets.value);
        Assert.Equal(_initial.Platform, resultAllSecrets.value[0].Platform);

        // --- UPDATE ---
        var resultUpdated = await _secretsManager.UpdateItemAsync(_initial.Platform, _updated);
        Assert.True(resultUpdated.value);

        var updatedSecrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Single(updatedSecrets.value);
        Assert.Equal(_updated.Secret, updatedSecrets.value[0].Secret);

        // --- DELETE ---
        var deletedResult = await _secretsManager.DeleteItemAsync(_updated.Platform);
        Assert.True(deletedResult.value);

        var resultAfterDelete = await _secretsManager.GetAllSecretsAsync();
        Assert.Empty(resultAfterDelete.value);
    }

    [Fact]
    public async Task AddNewItem_ShouldPersistSecret()
    {
        var resultAdded = await _secretsManager.AddNewItemAsync(_initial);
        Assert.True(resultAdded.value);

        var secrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Single(secrets.value);
        Assert.Equal(_initial.Platform, secrets.value[0].Platform);
    }

    [Fact]
    public async Task UpdateItem_ShouldReplaceSecret()
    {
        await _secretsManager.AddNewItemAsync(_initial);

        var updated = await _secretsManager.UpdateItemAsync(_initial.Platform, _updated);
        Assert.True(updated.value);

        var secrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Single(secrets.value);
        Assert.Equal(_updated.Secret, secrets.value[0].Secret);
    }

    [Fact]
    public async Task DeleteItem_ShouldRemoveSecret()
    {
        await _secretsManager.AddNewItemAsync(_initial);
        await _secretsManager.UpdateItemAsync(_initial.Platform, _updated);

        var deleted = await _secretsManager.DeleteItemAsync(_updated.Platform);
        Assert.True(deleted.value);

        var secrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Empty(secrets.value);
    }


    [Fact]
    public async Task BackupSecretsFile_ShouldCreateBackup()
    {
        _ = await _secretsManager.AddNewItemAsync(_initial);

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
