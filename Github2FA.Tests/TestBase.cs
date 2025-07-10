using Github2FA.Interfaces;
using Github2FA.ViewModels;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Github2FA.Tests
{
    public class TestBase
    {

        public static (
                  MainViewModel vm,
                  Mock<IClipboardService> clipboardMock,
                  Mock<IConfiguration> configMock,
                  Mock<ITotpManager> totpMock,
                  Mock<IDebounceService> debounceMock,
  Mock<IMessageService> msgMock,
Mock<IDelayService> delayMock
              ) GetVMWithMocks()
        {
            var msgMock = new Mock<IMessageService>();
            var clipboardMock = new Mock<IClipboardService>();
            var configMock = new Mock<IConfiguration>();
            var totpMock = new Mock<ITotpManager>();
            var debounceMock = new Mock<IDebounceService>();
            var delayMock = new Mock<IDelayService>();

            var vm = new MainViewModel(
                msgMock.Object,
                clipboardMock.Object,
                configMock.Object,
                totpMock.Object,
                debounceMock.Object,
                delayMock.Object
            );

            return (vm, clipboardMock, configMock, totpMock, debounceMock, msgMock, delayMock);
        }

    }
}
