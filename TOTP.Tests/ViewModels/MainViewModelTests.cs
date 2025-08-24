using Moq;
using Moq.AutoMock;
using System.Diagnostics;
using TOTP.Core.Common;
using TOTP.Core.Interfaces;
using TOTP.Core.Models;
using TOTP.Extensions;
using TOTP.Interfaces;
using TOTP.ViewModels;

namespace TOTP.Tests.ViewModels;

public class MainViewModelTests : IClassFixture<MyFixture>
{
    private readonly MyFixture _fixture;

    public MainViewModelTests(MyFixture fixture)
    {
        _fixture = fixture;
        Debug.WriteLine("MainViewModelTests constructor called. This method is called before each test.");
    }

    [Fact]
    public void AddNewSecretCommand_ShouldAddNewSecret_WhenManagerReturnsSuccess()
    {
        var mocker = new AutoMocker();
        SetupSecretsDataSourceMock(mocker);

        var secretItem = new SecretItemViewModel("TestKey", "TestValue");

        mocker.GetMock<ITotpManager>()
            .Setup(m => m.AddNewSecretAsync())
            .ReturnsAsync((true, secretItem.ToDomain()));

        var vm = mocker.CreateInstance<MainViewModel>();
        var initialCount = vm.AllSecrets.Count;

        vm.AddNewSecretCommand.Execute(null);

        Assert.Equal(initialCount + 1, vm.AllSecrets.Count);
        Assert.Contains(secretItem, vm.AllSecrets);
    }

    [Fact]
    public void DeleteSecretCommand_ShouldRemoveSecret_WhenManagerDeletesSuccessfully()
    {
        var mocker = new AutoMocker();
        SetupSecretsDataSourceMock(mocker);

        var vm = mocker.CreateInstance<MainViewModel>();

        var secret = new SecretItemViewModel("DeleteKey", "DeleteValue");
        mocker.GetMock<ITotpManager>().Setup(m => m.DeleteSecretAsync(secret.ToDomain())).Returns(Task.FromResult(true));

        vm.AllSecrets.Add(secret);
        var initialCount = vm.AllSecrets.Count;

        vm.DeleteSecretCommand.Execute(secret);

        Assert.Equal(initialCount - 1, vm.AllSecrets.Count);
        Assert.DoesNotContain(secret, vm.AllSecrets);
    }

    [Fact]
    public void SearchText_ShouldFilterSecrets()
    {
        var mocker = new AutoMocker();
        SetupSecretsDataSourceMock(mocker);
        var vm = mocker.CreateInstance<MainViewModel>();

        vm.AllSecrets.Add(new SecretItemViewModel("apple", "value1"));
        vm.AllSecrets.Add(new SecretItemViewModel("banana", "value2"));

        vm.SearchText = "apple";
        vm.UpdateSearchFilter(); // made method internal and set [assembly:InternalsVisibleTo(..) in TOTP.csproj asemblyinfo.cs

        // or use reflection to call the private method
        //var method = typeof(MainViewModel).GetMethod("UpdateSearchFilter", BindingFlags.NonPublic | BindingFlags.Instance);
        //method?.Invoke(vm, null);

        Assert.Single(vm.FilteredSecrets);
        Assert.Equal("apple", vm.FilteredSecrets[0].Platform);
    }

    [Fact]
    public void ClearSearchCommand_ShouldClearSearchText()
    {
        var mocker = new AutoMocker();
        SetupSecretsDataSourceMock(mocker);

        var vm = mocker.CreateInstance<MainViewModel>();
        vm.SearchText = "query";

        vm.ClearSearchCommand.Execute(null);

        Assert.True(string.IsNullOrEmpty(vm.SearchText));
    }

    [Fact]
    public void BeginEditCommand_ShouldSetIsBeingEdited()
    {
        var mocker = new AutoMocker();
        SetupSecretsDataSourceMock(mocker);
        var vm = mocker.CreateInstance<MainViewModel>();

        var secret = new SecretItemViewModel("key", "value");
        vm.AllSecrets.Add(secret);

        vm.BeginEditCommand.Execute(secret);

        Assert.True(secret.IsBeingEdited);
    }

    [Fact]
    public void EndEditCommand_ShouldUpdateSecret_AndResetPreviousVersion()
    {
        var mocker = new AutoMocker();
        SetupSecretsDataSourceMock(mocker);
        var vm = mocker.CreateInstance<MainViewModel>();

        var oldSecret = new SecretItemViewModel("key", "old");
        var updatedSecret = new SecretItemViewModel("key", "new");
        vm.AllSecrets.Add(updatedSecret);
        vm.PreviousVersion = oldSecret;

        vm.EndEditCommand.Execute(updatedSecret);
        var secretList = vm.AllSecrets.Select(s => s.ToDomain()).ToList();
        mocker.GetMock<ITotpManager>().Verify(m => m.UpdateSecretAsync(oldSecret.ToDomain(), updatedSecret.ToDomain(), new List<SecretItem>()), Times.Once);
        Assert.Null(vm.PreviousVersion);
    }

    [Fact]
    public void ToggleSearchBoxCommand_ShouldToggleSearchVisibility()
    {
        var mocker = new AutoMocker();
        SetupSecretsDataSourceMock(mocker);
        var vm = mocker.CreateInstance<MainViewModel>();

        var initial = vm.IsSearchVisible;
        vm.ToggleSearchBoxCommand.Execute(null);

        Assert.NotEqual(initial, vm.IsSearchVisible);
    }

    [Fact]
    public void DoubleClickCommand_ShouldEnableEditMode()
    {
        var mocker = new AutoMocker();
        SetupSecretsDataSourceMock(mocker);
        var vm = mocker.CreateInstance<MainViewModel>();

        var secret = new SecretItemViewModel("key", "value");
        vm.AllSecrets.Add(secret);

        vm.DoubleClickCommand.Execute(secret);

        Assert.True(secret.IsBeingEdited);
    }

    [Fact]
    public void UpdateSecretCommand_ShouldCallUpdate_WhenSecretChanged()
    {
        var mocker = new AutoMocker();
        SetupSecretsDataSourceMock(mocker);
        var vm = mocker.CreateInstance<MainViewModel>();

        var old = new SecretItemViewModel("p1", "old");
        var updated = new SecretItemViewModel("p1", "new");

        vm.PreviousVersion = old;
        vm.UpdateSecretCommand.Execute(updated);
        var secretList = vm.AllSecrets.Select(s => s.ToDomain()).ToList();
        mocker.GetMock<ITotpManager>().Verify(m => m.UpdateSecretAsync(old.ToDomain(), updated.ToDomain(), secretList), Times.Once);
    }

    [Fact]
    public void SearchText_ShouldRaisePropertyChanged()
    {
        var mocker = new AutoMocker();
        SetupSecretsDataSourceMock(mocker);
        var vm = mocker.CreateInstance<MainViewModel>();

        var eventRaised = false;
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(vm.SearchText))
                eventRaised = true;
        };

        vm.SearchText = "test";

        Assert.True(eventRaised);
    }

    [StaFact]
    public async Task OnSecretSelected_ShouldGenerateTotp_AndCopyToClipboard()
    {
        var secret = new SecretItemViewModel("TestPlatform", "JBSWY3DPEHPK3PXP");

        var mocker = new AutoMocker();
        SetupSecretsDataSourceMock(mocker);

        var vm = mocker.CreateInstance<MainViewModel>();

        string? capturedText = null;

        mocker.GetMock<IQrCodeService>()
            .Setup(qs => qs.BuildOtpAuthUri(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("otpauth://totp/Test:acc?secret=JBSWY3DPEHPK3PXP&issuer=Test&algorithm=SHA1&digits=6&period=30");

        static byte[] OneByOnePng()
        {
            using var bmp = new System.Drawing.Bitmap(1, 1);
            using var ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }


        mocker.GetMock<IQrCodeService>()
            .Setup(qs => qs.GenerateQr(It.IsAny<string>()))
            .Returns(OneByOnePng());


        mocker.GetMock<IClipboardService>().Setup(c => c.SetText(It.IsAny<string>()))
            .Callback<string>(text => capturedText = text);

#pragma warning disable CS8601
        mocker.GetMock<ITotpManager>().Setup(m =>
                m.TryComputeCode(It.IsAny<string>(), out It.Ref<string>.IsAny, out It.Ref<Exception>.IsAny))
            .Returns(true)
            .Callback((string input, out string code, out Exception? error) =>
            {
                code = "123456";
                error = null;
            });
#pragma warning restore CS8601

        var delayMock = mocker.GetMock<IDelayService>();
        delayMock.Setup(d => d.Delay(It.IsAny<int>())).Returns(Task.CompletedTask);

        vm.SelectedSecret = secret;

        await vm.OnSecretSelectedAsync();

        delayMock.Verify(d => d.Delay(It.IsAny<int>()), Times.Once);

        Assert.Contains("TestPlatform", vm.CurrentCodeLabel);
        Assert.False(vm.IsCodeCopiedVisible);
    }

    private static void SetupSecretsDataSourceMock(AutoMocker mocker)
    {
        var secrets = new List<SecretItemViewModel>();
        var secretsManagerMock = new Mock<ISecretsManager>();
        secretsManagerMock.Setup(m => m.GetAllSecretsAsync()).ReturnsAsync(Result<List<SecretItem>>.Success(secrets.Select(sivm => sivm.ToDomain()).ToList()));
        mocker.Use<ISecretsManager>(secretsManagerMock.Object);
    }


}
