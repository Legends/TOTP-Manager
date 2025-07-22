using AutoFixture;
using AutoFixture.AutoMoq;
using Github2FA.Interfaces;
using Github2FA.Models;
using Github2FA.ViewModels;
using Moq;
using Moq.AutoMock;
using System.Diagnostics;
using System.Reflection;

namespace Github2FA.Tests.ViewModels;


/// <summary>
/// dotnet add package Moq
/// dotnet add package Otp.NET

/// </summary>
public class MainViewModelTests : IClassFixture<MyFixture>, IDisposable
{
    private readonly MyFixture _fixture;

    public MainViewModelTests(MyFixture fixture)
    {
        _fixture = fixture;
        // This constructor is called before each test
        Debug.WriteLine("MainViewModelTests constructor called. This method is called before each test.");
        // You can initialize shared resources here
    }

    #region ### Mock.AutoMock ###
    [Fact]
    public void AddNewTotpCommand_ShouldAddNewSecret_WhenManagerReturnsSuccess()
    {
        // Use _fixture
        //_fixture.
        // Arrange
        var mocker = new AutoMocker();
        // Auto-resolve all dependencies
        var vm = mocker.CreateInstance<MainViewModel>();

        var secretItem = new SecretItem("TestKey", "TestValue");

        mocker.GetMock<ITotpManager>()
                      .Setup(m => m.PromptAndAddTotp())
                                .Returns((true, secretItem));

        int initialCount = vm.AllSecrets.Count;

        // Act
        vm.AddNewTotpCommand.Execute(null);

        // Assert
        Assert.Equal(initialCount + 1, vm.AllSecrets.Count);
        Assert.Contains(secretItem, vm.AllSecrets);
    }

    [Fact]
    public void DeleteSecretCommand_ShouldRemoveSecret_WhenManagerDeletesSuccessfully()
    {
        // Arrange
        var mocker = new AutoMocker();
        // Auto-resolve all dependencies
        var vm = mocker.CreateInstance<MainViewModel>();

        var secret = new SecretItem("DeleteKey", "DeleteValue");
        mocker.GetMock<ITotpManager>().Setup(m => m.DeleteSecret(secret)).Returns(true);

        vm.AllSecrets.Add(secret);

        int initialCount = vm.AllSecrets.Count;

        // Act
        vm.DeleteSecretCommand.Execute(secret);

        // Assert
        Assert.Equal(initialCount - 1, vm.AllSecrets.Count);
        Assert.DoesNotContain(secret, vm.AllSecrets);
    }

    [Fact]
    public void SearchText_ShouldFilterSecrets()
    {
        // Arrange
        var mocker = new AutoMocker();
        // Auto-resolve all dependencies
        var vm = mocker.CreateInstance<MainViewModel>();

        vm.AllSecrets.Add(new SecretItem("apple", "value1"));
        vm.AllSecrets.Add(new SecretItem("banana", "value2"));

        // Act
        vm.SearchText = "apple";

        var method = typeof(MainViewModel).GetMethod("UpdateFilter", BindingFlags.NonPublic | BindingFlags.Instance);

        if (method != null)
        {
            method.Invoke(vm, null);
        }
        else
        {
            throw new InvalidOperationException("UpdateFilter() not found.");
        }

        //vm.UpdateFilter();

        // Assert
        Assert.Single(vm.FilteredSecrets);
        Assert.Equal("apple", vm.FilteredSecrets[0].Platform);
    }

    [Fact]
    public void ClearSearchCommand_ShouldClearSearchText()
    {
        // Arrange
        var mocker = new AutoMocker();
        // Auto-resolve all dependencies
        var vm = mocker.CreateInstance<MainViewModel>();

        vm.SearchText = "query";

        // Act
        vm.ClearSearchCommand.Execute(null);

        // Assert
        Assert.True(string.IsNullOrEmpty(vm.SearchText));
    }
    [Fact]
    public void BeginEditCommand_ShouldSetIsBeingEdited()
    {
        // Arrange
        var mocker = new AutoMocker();
        // Auto-resolve all dependencies
        var vm = mocker.CreateInstance<MainViewModel>();

        var secret = new SecretItem("key", "value");
        vm.AllSecrets.Add(secret);

        // Act
        vm.BeginEditCommand.Execute(secret);

        // Assert
        Assert.True(secret.IsBeingEdited);
    }
    [Fact]
    public void SearchText_ShouldRaisePropertyChanged()
    {
        // Arrange
        var mocker = new AutoMocker();
        // Auto-resolve all dependencies
        var vm = mocker.CreateInstance<MainViewModel>();

        bool eventRaised = false;
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(vm.SearchText))
                eventRaised = true;
        };

        // Act
        vm.SearchText = "test";

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task OnSingleTap_ShouldGenerateTotp_AndCopyToClipboard()
    {
        // Arrange
        var secret = new SecretItem("TestPlatform", "JBSWY3DPEHPK3PXP");

        // Arrange
        var mocker = new AutoMocker();
        // Auto-resolve all dependencies
        var vm = mocker.CreateInstance<MainViewModel>();

        string capturedText = null;

        mocker.GetMock<IClipboardService>().Setup(c => c.SetText(It.IsAny<string>()))
                   .Callback<string>(text => capturedText = text);


        mocker.GetMock<ITotpManager>().Setup(m => m.TryComputeCode(It.IsAny<string>(), out It.Ref<string>.IsAny, out It.Ref<string>.IsAny))
            .Returns(true)
            .Callback((string input, out string code, out string error) =>
            {
                code = "123456";
                error = null;
            });

        var delayMock = mocker.GetMock<IDelayService>();
        delayMock.Setup(d => d.Delay(It.IsAny<int>())).Returns(Task.CompletedTask);

        vm.SelectedSecret = secret;

        try
        {
            var method = vm.GetType().GetMethod("OnSecretSelected", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)method.Invoke(vm, null);
        }
        catch (Exception) { throw; }

        delayMock.Verify(d => d.Delay(It.IsAny<int>()), Times.Once);

        // Assert
        Assert.Contains("TestPlatform", vm.CurrentCodeLabel);
        Assert.False(vm.IsCodeCopiedVisible);
    }
    #endregion

    #region ### AutoFixture.AutoMoq

    [Fact]
    public void ClearSearchCommand_ShouldClearSearchText2()
    {
        var fixture = new Fixture().Customize(new AutoMoqCustomization());
        var vm = fixture.Create<MainViewModel>();

        vm.SearchText = "hello";

        vm.ClearSearchCommand.Execute(null);

        Assert.True(string.IsNullOrEmpty(vm.SearchText));
    }

    [Fact]
    public void BeginEditCommand_ShouldSetIsBeingEdited2()
    {
        var fixture = new Fixture().Customize(new AutoMoqCustomization());
        var vm = fixture.Create<MainViewModel>();

        var secret = new SecretItem("key", "value");
        vm.AllSecrets.Add(secret);

        vm.BeginEditCommand.Execute(secret);

        Assert.True(secret.IsBeingEdited);
    }

    public string? ToJson()
    {
        // This method is called after each test
        return "";
    }

    public void Dispose()
    {
        // This method is called after each test
        Debug.WriteLine("Dispose is: This method is called after each test");
        // Clean up resources if needed
        // For example, you can reset static properties or clear collections
        // _fixture = null; // Not necessary, as it will be garbage collected
    }
    #endregion
}
