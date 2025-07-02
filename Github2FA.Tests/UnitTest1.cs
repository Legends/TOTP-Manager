using Github2FA.Interfaces;
using Github2FA.Models;
using Github2FA.ViewModels;
using Microsoft.Extensions.Configuration;
using Moq;
using static System.Net.WebRequestMethods;

namespace Github2FA.Tests
{

    /// <summary>
    /// dotnet add package Moq
    /// dotnet add package Otp.NET

/// </summary>
    public class UnitTest1
    {
        [Fact]
        public void AddNewTotp_ShouldAddValidSecret()
        {
            // Arrange
            var dialogServiceMock = new Mock<IDialogService>();
            dialogServiceMock.Setup(d => d.ShowKeyValueDialog(null, null))
                .Returns((true, "MyKey", "JBSWY3DPEHPK3PXP")); // Valid Base32

            var messageServiceMock = new Mock<IMessageService>();

            var secretsHelperMock = new Mock<ISecretsHelper>();
            secretsHelperMock.Setup(s => s.AddNewItemToSecretsFile(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var vm = new MainViewModel(dialogServiceMock.Object,
                                        new Mock<IConfiguration>().Object,
                                        messageServiceMock.Object,
                                        secretsHelperMock.Object);



            // Act
            var initialCount = vm.AllSecrets.Count;
            var method = typeof(MainViewModel).GetMethod("addNewTotp", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method.Invoke(vm, null);

            // Assert
            Assert.Equal(initialCount + 1, vm.AllSecrets.Count);
            secretsHelperMock.Verify(s => s.AddNewItemToSecretsFile("MyKey", "JBSWY3DPEHPK3PXP"), Times.Once);
        }

        [Fact]
        public void AddNewTotp_ShouldShowError_WhenBase32Invalid()
        {
            // Arrange
            var dialogServiceMock = new Mock<IDialogService>();
            dialogServiceMock.Setup(d => d.ShowKeyValueDialog(null, null))
                .Returns((true, "MyKey", "INVALID_BASE32"));

            var messageServiceMock = new Mock<IMessageService>();

            var secretsHelperMock = new Mock<ISecretsHelper>();

            var vm = new MainViewModel(dialogServiceMock.Object,
                                        new Mock<IConfiguration>().Object,
                                        messageServiceMock.Object,
                                        secretsHelperMock.Object);

            // Act
            var method = typeof(MainViewModel).GetMethod("addNewTotp", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method.Invoke(vm, null);

            // Assert
            messageServiceMock.Verify(m => m.ShowMessage(
                "Secret must be a valid Base32 string.", "Error"), Times.Once);

            Assert.Empty(vm.AllSecrets);
        }

        [Fact]
        public void DeleteSecret_ShouldRemoveSecret_WhenConfirmed()
        {
            // Arrange
            var messageServiceMock = new Mock<IMessageService>();
            messageServiceMock.Setup(m => m.ShowMessageDialog(It.IsAny<string>(), It.IsAny<string>()))
                              .Returns(true);

            var secretsHelperMock = new Mock<ISecretsHelper>();

            var secret = new SecretItem("MyKey", "JBSWY3DPEHPK3PXP");

            var vm = new MainViewModel(
                new Mock<IDialogService>().Object,
                new Mock<IConfiguration>().Object,
                messageServiceMock.Object,
                secretsHelperMock.Object);

            vm.AllSecrets.Add(secret);

            // Act
            vm.DeleteSecret(secret);

            // Assert
            Assert.Empty(vm.AllSecrets);
            secretsHelperMock.Verify(s => s.DeleteItemFromSecretsFile("MyKey"), Times.Once);
        }

        [Fact]
        public async Task OnSecretSelected_ShouldGenerateCode()
        {
            // Arrange
            var vm = new MainViewModel(
                new Mock<IDialogService>().Object,
                new Mock<IConfiguration>().Object,
                new Mock<IMessageService>().Object,
                new Mock<ISecretsHelper>().Object);

            var secret = new SecretItem("TestPlatform", "JBSWY3DPEHPK3PXP");

            vm.SelectedSecret = secret;

            // Wait briefly to let async method complete
            await Task.Delay(100);

            // Assert
            Assert.Contains("TestPlatform", vm.CurrentCodeLabel);
            Assert.Matches(@"\d{6}$", vm.CurrentCodeLabel); // TOTP codes are 6 digits
        }


    }
}
