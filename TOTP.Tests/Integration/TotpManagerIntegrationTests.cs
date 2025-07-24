using Moq;
using Moq.AutoMock;
using TOTP.Interfaces;
using TOTP.Models;
using TOTP.Services;

namespace TOTP.Tests.Integration;

public class TotpManagerIntegrationTests : IDisposable
{
    private readonly string _testPath;
    private readonly AutoMocker _mocker;
    private readonly SecretsManager _secretsManager;
    private readonly TotpManager _totpManager;

    private readonly SecretItem _initialSecret = new("GitHub", "JBSWY3DPEHPK3PXP");

    public TotpManagerIntegrationTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), $"secrets-test-{Guid.NewGuid()}.dat");

        _mocker = new AutoMocker();

        // Set up mocks
        _mocker.GetMock<IDialogService>()
            .Setup(x => x.ShowKeyValueDialog(It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns((true, _initialSecret.Platform, _initialSecret.Secret));

        _mocker.GetMock<IMessageService>()
            .Setup(x => x.ShowMessageDialog(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true); // Simulate confirmation for deletion

        // Real SecretsManager with test file
        _secretsManager = new SecretsManager(_mocker.Get<IMessageService>(), _testPath);
        _mocker.Use<ISecretsManager>(_secretsManager);

        // Create real TotpManager
        _totpManager = _mocker.CreateInstance<TotpManager>();
    }

    [Fact]
    public void Add_Compute_Update_Delete_Secret_ShouldSucceed()
    {
        // --- ADD ---
        var (success, item) = _totpManager.AddNewSecret();
        Assert.True(success);
        Assert.NotNull(item);

        var secrets = _secretsManager.GetAllSecrets();
        Assert.Single(secrets);
        Assert.Equal(_initialSecret.Platform, secrets[0].Platform);

        // --- COMPUTE ---
        var codeResult = _totpManager.TryComputeCode(_initialSecret.Secret, out var code, out var error);
        Assert.True(codeResult);
        Assert.Matches(@"^\d{6}$", code!);
        Assert.Null(error);

        // --- UPDATE ---
        var updated = new SecretItem(_initialSecret.Platform, "MZXW6YTBOI======");
        _totpManager.UpdateSecret(_initialSecret, updated);

        var updatedSecrets = _secretsManager.GetAllSecrets();
        Assert.Single(updatedSecrets);
        Assert.Equal("MZXW6YTBOI======", updatedSecrets[0].Secret);

        // --- DELETE ---
        var deleteResult = _totpManager.DeleteSecret(updated);
        Assert.True(deleteResult);
        Assert.Empty(_secretsManager.GetAllSecrets());
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
