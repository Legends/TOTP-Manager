namespace TOTP.Tests;

public class TestBase
{
    /// <summary>
    /// when not using mocks
    /// </summary>
    //[Fact]
    //public void AddNewTotpCommand_ShouldAddNewSecret_WhenManagerReturnsSuccess()
    //{
    //    var msgMock = new Mock<IMessageService>();
    //    var clipboardMock = new Mock<IClipboardService>();
    //    var configMock = new Mock<IConfiguration>();
    //    var totpMock = new Mock<ITotpManager>();
    //    var debounceMock = new Mock<IDebounceService>();
    //    var delayMock = new Mock<IDelayService>();

    //    var vm = new MainViewModel(
    //        msgMock.Object,
    //        clipboardMock.Object,
    //        configMock.Object,
    //        totpMock.Object,
    //        debounceMock.Object,
    //        delayMock.Object
    //    );

    //    var secretItem = new SecretItem("TestKey", "TestValue");

    //    totpMock.Setup(m => m.PromptAndAddTotp())
    //                            .Returns((true, secretItem));

    //    int initialCount = vm.AllSecrets.Count;

    //    //Act
    //    vm.AddNewTotpCommand.Execute(null);

    //    //Assert
    //    Assert.Equal(initialCount + 1, vm.AllSecrets.Count);
    //    Assert.Contains(secretItem, vm.AllSecrets);
    //}
}