using Moq.AutoMock;
using TOTP.Interfaces;
using TOTP.Models;
using TOTP.Services;

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
    public void FullLifecycle_AddUpdateDelete_ShouldSucceed()
    {
        // --- ADD ---
        var added = _secretsManager.AddNewItem(_initial);
        Assert.True(added);

        var secrets = _secretsManager.GetAllSecrets();
        Assert.Single(secrets);
        Assert.Equal(_initial.Platform, secrets[0].Platform);

        // --- UPDATE ---
        var updated = _secretsManager.UpdateItem(_initial.Platform, _updated);
        Assert.True(updated);

        var updatedSecrets = _secretsManager.GetAllSecrets();
        Assert.Single(updatedSecrets);
        Assert.Equal(_updated.Secret, updatedSecrets[0].Secret);

        // --- DELETE ---
        var deleted = _secretsManager.DeleteItem(_updated.Platform);
        Assert.True(deleted);

        var afterDelete = _secretsManager.GetAllSecrets();
        Assert.Empty(afterDelete);
    }

    [Fact]
    public void AddNewItem_ShouldPersistSecret()
    {
        var added = _secretsManager.AddNewItem(_initial);
        Assert.True(added);

        var secrets = _secretsManager.GetAllSecrets();
        Assert.Single(secrets);
        Assert.Equal(_initial.Platform, secrets[0].Platform);
    }

    [Fact]
    public void UpdateItem_ShouldReplaceSecret()
    {
        _secretsManager.AddNewItem(_initial);

        var updated = _secretsManager.UpdateItem(_initial.Platform, _updated);
        Assert.True(updated);

        var secrets = _secretsManager.GetAllSecrets();
        Assert.Single(secrets);
        Assert.Equal(_updated.Secret, secrets[0].Secret);
    }

    [Fact]
    public void DeleteItem_ShouldRemoveSecret()
    {
        _secretsManager.AddNewItem(_initial);
        _secretsManager.UpdateItem(_initial.Platform, _updated);

        var deleted = _secretsManager.DeleteItem(_updated.Platform);
        Assert.True(deleted);

        var secrets = _secretsManager.GetAllSecrets();
        Assert.Empty(secrets);
    }


    [Fact]
    public void BackupSecretsFile_ShouldCreateBackup()
    {
        _secretsManager.AddNewItem(_initial);

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
