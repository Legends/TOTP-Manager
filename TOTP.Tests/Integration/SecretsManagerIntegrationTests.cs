using Moq.AutoMock;
using TOTP.Interfaces;
using TOTP.Models;

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
            _mocker.Get<IMessageService>(),
            _testPath
        );
    }

    [Fact]
    public async Task FullLifecycle_AddUpdateDelete_ShouldSucceed()
    {
        // --- ADD ---
        var added = await _secretsManager.AddNewItemAsync(_initial);
        Assert.True(added);

        var secrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Single(secrets);
        Assert.Equal(_initial.Platform, secrets[0].Platform);

        // --- UPDATE ---
        var updated = await _secretsManager.UpdateItemAsync(_initial.Platform, _updated);
        Assert.True(updated);

        var updatedSecrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Single(updatedSecrets);
        Assert.Equal(_updated.Secret, updatedSecrets[0].Secret);

        // --- DELETE ---
        var deleted = await _secretsManager.DeleteItemAsync(_updated.Platform);
        Assert.True(deleted);

        var afterDelete = await _secretsManager.GetAllSecretsAsync();
        Assert.Empty(afterDelete);
    }

    [Fact]
    public async Task AddNewItem_ShouldPersistSecret()
    {
        var added = await _secretsManager.AddNewItemAsync(_initial);
        Assert.True(added);

        var secrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Single(secrets);
        Assert.Equal(_initial.Platform, secrets[0].Platform);
    }

    [Fact]
    public async Task UpdateItem_ShouldReplaceSecret()
    {
        await _secretsManager.AddNewItemAsync(_initial);

        var updated = await _secretsManager.UpdateItemAsync(_initial.Platform, _updated);
        Assert.True(updated);

        var secrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Single(secrets);
        Assert.Equal(_updated.Secret, secrets[0].Secret);
    }

    [Fact]
    public async Task DeleteItem_ShouldRemoveSecret()
    {
        await _secretsManager.AddNewItemAsync(_initial);
        await _secretsManager.UpdateItemAsync(_initial.Platform, _updated);

        var deleted = await _secretsManager.DeleteItemAsync(_updated.Platform);
        Assert.True(deleted);

        var secrets = await _secretsManager.GetAllSecretsAsync();
        Assert.Empty(secrets);
    }


    [Fact]
    public async Task BackupSecretsFile_ShouldCreateBackup()
    {
        await _secretsManager.AddNewItemAsync(_initial);

        var backupSuccess = await _secretsManager.BackupSecretsFileAsync();
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
