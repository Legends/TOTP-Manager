using System.Text;
using System.Windows.Forms;

var logDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "TOTP-Manager",
    "UpdateStub");
Directory.CreateDirectory(logDirectory);

var logPath = Path.Combine(logDirectory, "launch.log");
var lines = new[]
{
    $"utc={DateTimeOffset.UtcNow:O}",
    $"pid={Environment.ProcessId}",
    $"exe={Environment.ProcessPath}",
    $"cwd={Environment.CurrentDirectory}",
    $"args={string.Join(" ", args)}",
    new string('-', 60)
};
File.AppendAllText(logPath, string.Join(Environment.NewLine, lines) + Environment.NewLine, Encoding.UTF8);

MessageBox.Show(
    "Local update stub launched successfully.\n\nThis proves NetSparkle downloaded the payload and executed it.\n\nCheck the launch log for the exact command line:\n" + logPath,
    "TOTP Local Update Stub",
    MessageBoxButtons.OK,
    MessageBoxIcon.Information);
