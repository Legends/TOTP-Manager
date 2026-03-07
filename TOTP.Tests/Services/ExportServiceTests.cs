using TOTP.Core.Common;
using TOTP.Core.Models;
using TOTP.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace TOTP.Tests.Services;

public sealed class ExportServiceTests
{
    private readonly ExportService _sut = new(NullLogger<ExportService>.Instance);

    [Theory]
    [InlineData(ExportFileFormat.Json, ".json")]
    [InlineData(ExportFileFormat.Txt, ".txt")]
    [InlineData(ExportFileFormat.Csv, ".csv")]
    public async Task ExportToFileAsync_ThenImportFromFileAsync_RoundTripsSupportedFormats(ExportFileFormat format, string extension)
    {
        using var temp = new TempDir();
        var path = Path.Combine(temp.Path, "accounts" + extension);
        var id = Guid.NewGuid();
        List<Account> input =
        [
            new(id, "GitHub, Inc.", "AAAABBBB", "john\"doe"),
            new(Guid.NewGuid(), "Google", "CCCCDDDD")
        ];

        var export = await _sut.ExportToFileAsync(input, path, format);
        var import = await _sut.ImportFromFileAsync(path);

        Assert.True(export.IsSuccess);
        Assert.True(import.IsSuccess);
        Assert.Equal(2, import.Value.Count);
        Assert.Equal(id, import.Value[0].ID);
        Assert.Equal("GitHub, Inc.", import.Value[0].Issuer);
        Assert.Equal("AAAABBBB", import.Value[0].Secret);
        Assert.Equal("john\"doe", import.Value[0].AccountName);
    }

    [Fact]
    public async Task ImportFromFileAsync_WhenExtensionUnsupported_ReturnsInvalidFileError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var temp = new TempDir();
        var path = Path.Combine(temp.Path, "accounts.xml");
        await File.WriteAllTextAsync(path, "<accounts/>", cancellationToken);

        var result = await _sut.ImportFromFileAsync(path);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorCode.ImportInvalidFile, result.GetErrorCode());
    }

    [Fact]
    public async Task ImportFromFileAsync_WhenFileMissing_ReturnsFileNotFoundError()
    {
        using var temp = new TempDir();
        var path = Path.Combine(temp.Path, "missing.json");

        var result = await _sut.ImportFromFileAsync(path);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorCode.ImportFileNotFound, result.GetErrorCode());
    }

    [Fact]
    public async Task ImportFromFileAsync_WhenFileTooLarge_ReturnsInvalidFileError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var temp = new TempDir();
        var path = Path.Combine(temp.Path, "oversized.json");
        var oversized = new string('A', 6 * 1024 * 1024);
        await File.WriteAllTextAsync(path, oversized, cancellationToken);

        var result = await _sut.ImportFromFileAsync(path);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorCode.ImportInvalidFile, result.GetErrorCode());
    }

    [Fact]
    public async Task ImportFromFileAsync_WhenEncryptedWithoutPassword_ReturnsWrongPasswordError()
    {
        using var temp = new TempDir();
        var encrypted = Path.Combine(temp.Path, "accounts.totp");
        var export = await _sut.ExportToEncryptedFileAsync(
            [new Account(Guid.NewGuid(), "GitHub", "SECRET")],
            "correct-password",
            encrypted,
            ExportFileFormat.Json);
        Assert.True(export.IsSuccess);

        var result = await _sut.ImportFromFileAsync(encrypted, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorCode.ImportWrongPasswordOrTampered, result.GetErrorCode());
    }

    [Theory]
    [InlineData(ExportFileFormat.Json)]
    [InlineData(ExportFileFormat.Txt)]
    [InlineData(ExportFileFormat.Csv)]
    public async Task ExportToEncryptedFileAsync_ThenImportFromEncryptedFileAsync_RoundTrips(ExportFileFormat format)
    {
        using var temp = new TempDir();
        var path = Path.Combine(temp.Path, "accounts.totp");
        List<Account> input = [new(Guid.NewGuid(), "Azure", "ABCD1234", "tenant-user")];

        var export = await _sut.ExportToEncryptedFileAsync(input, "pw-123", path, format);
        var import = await _sut.ImportFromEncryptedFileAsync("pw-123", path);

        Assert.True(export.IsSuccess);
        Assert.True(import.IsSuccess);
        var token = Assert.Single(import.Value);
        Assert.Equal("Azure", token.Issuer);
        Assert.Equal("ABCD1234", token.Secret);
        Assert.Equal("tenant-user", token.AccountName);
    }

    [Fact]
    public async Task ImportFromEncryptedFileAsync_WhenPasswordWrong_ReturnsWrongPasswordOrTampered()
    {
        using var temp = new TempDir();
        var path = Path.Combine(temp.Path, "accounts.totp");
        Assert.True((await _sut.ExportToEncryptedFileAsync(
            [new Account(Guid.NewGuid(), "Google", "XYZ")],
            "right-password",
            path,
            ExportFileFormat.Json)).IsSuccess);

        var result = await _sut.ImportFromEncryptedFileAsync("wrong-password", path);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorCode.ImportWrongPasswordOrTampered, result.GetErrorCode());
    }

    [Fact]
    public async Task ImportFromEncryptedFileAsync_WhenHeaderInvalid_ReturnsInvalidFile()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var temp = new TempDir();
        var path = Path.Combine(temp.Path, "invalid.totp");
        await File.WriteAllBytesAsync(path, "not-a-valid-header"u8.ToArray(), cancellationToken);

        var result = await _sut.ImportFromEncryptedFileAsync("pw", path);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorCode.ImportInvalidFile, result.GetErrorCode());
    }

    [Fact]
    public async Task ImportFromEncryptedFileAsync_WhenFileTooLarge_ReturnsInvalidFile()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var temp = new TempDir();
        var path = Path.Combine(temp.Path, "oversized.totp");
        await File.WriteAllBytesAsync(path, new byte[6 * 1024 * 1024], cancellationToken);

        var result = await _sut.ImportFromEncryptedFileAsync("pw", path);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorCode.ImportInvalidFile, result.GetErrorCode());
    }

    [Fact]
    public async Task ExportToEncryptedFileAsync_WhenDirectoryMissing_ReturnsWriteFailed()
    {
        using var temp = new TempDir();
        var path = Path.Combine(temp.Path, "missing", "accounts.totp");

        var result = await _sut.ExportToEncryptedFileAsync(
            [new Account(Guid.NewGuid(), "GitHub", "SECRET")],
            "pw",
            path,
            ExportFileFormat.Json);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorCode.ExportFileWriteFailed, result.GetErrorCode());
    }

    private sealed class TempDir : IDisposable
    {
        public TempDir()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "totp-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, recursive: true);
                }
            }
            catch
            {
                // best-effort test cleanup
            }
        }
    }
}
