using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class UpdateCheckViewModel : INotifyPropertyChanged
{
    internal const string TestAppcast = "<?xml version=\"1.0\" encoding=\"utf-8\"?><rss version=\"2.0\" xmlns:sparkle=\"http://www.andymatuschak.org/xml-namespaces/sparkle\"><channel><item><title>TOTP Manager M3 Test</title><sparkle:version>99.0.0</sparkle:version><enclosure url=\"https://example.invalid/totp-manager-m3-test.zip\" sparkle:version=\"99.0.0\" /></item></channel></rss>";
    internal const string TestPublicKey = "A6EHv/POEL4dcN0Y50vAmWfk1jCbpQ1fHdyGZBJVMbg=";
    internal const string TestSignature = "sqd9vwOpK+U2OJJMdQQBKN+RQCnUmv6uaYLuLSwiZISFj5ZS0fg/jylTSjL5vWwOYRjtHm4MGJEoQn19JChZCg==";

    private readonly ISignedAppcastVerifier _verifier;
    private readonly AsyncCommand _checkCommand;
    private string _message = "The M3 probe verifies a local signed test appcast and never downloads its artifact.";

    public UpdateCheckViewModel(ISignedAppcastVerifier verifier)
    {
        _verifier = verifier ?? throw new ArgumentNullException(nameof(verifier));
        _checkCommand = new AsyncCommand(CheckAsync, () => true);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand CheckCommand => _checkCommand;

    public string Message
    {
        get => _message;
        private set
        {
            if (_message == value) return;
            _message = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
        }
    }

    public Task CheckAsync()
    {
        var appcastBytes = Encoding.UTF8.GetBytes(TestAppcast);
        try
        {
            var result = _verifier.Verify(new SignedAppcastCheckRequest(
                appcastBytes,
                TestSignature,
                TestPublicKey,
                new Version(2, 0, 0),
                CurrentOperatingSystem(),
                System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()));
            Message = result.Status switch
            {
                SignedAppcastCheckStatus.UpdateAvailable =>
                    $"Signed test appcast accepted; version {result.Version} was selected. No download was started.",
                SignedAppcastCheckStatus.NoApplicableUpdate =>
                    "The signed test appcast is valid but has no applicable update.",
                SignedAppcastCheckStatus.InvalidSignature =>
                    "The test appcast signature was rejected.",
                _ => "The signed test appcast format was rejected."
            };
        }
        finally
        {
            System.Security.Cryptography.CryptographicOperations.ZeroMemory(appcastBytes);
        }

        return Task.CompletedTask;
    }

    private static string CurrentOperatingSystem() =>
        OperatingSystem.IsWindows() ? "windows" :
        OperatingSystem.IsMacOS() ? "macos" :
        OperatingSystem.IsLinux() ? "linux" : "unknown";
}
