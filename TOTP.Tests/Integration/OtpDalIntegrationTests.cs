using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Cryptography;
using System.Text;
using TOTP.Core.Common;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.DAL.Services;

namespace TOTP.Tests.Integration;

public sealed class OtpDalIntegrationTests
{
    [Fact]
    public async Task GetAllAsync_WhenStorageMissing_ReturnsEmptyList()
    {
        using var temp = new TempDir();
        var storagePath = Path.Combine(temp.Path, "master.totp");
        var sut = CreateSut(storagePath, new EchoVaultService());

        var result = await sut.GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task AddUpdateDelete_RoundTripFlow_PersistsExpectedEntries()
    {
        using var temp = new TempDir();
        var storagePath = Path.Combine(temp.Path, "master.totp");
        var sut = CreateSut(storagePath, new EchoVaultService());

        var id = Guid.NewGuid();
        var created = new OtpEntry(id, "GitHub", "AAAA", "john");

        Assert.True((await sut.AddNewAsync(created)).IsSuccess);

        var afterCreate = await sut.GetAllAsync();
        Assert.True(afterCreate.IsSuccess);
        var createdEntry = Assert.Single(afterCreate.Value);
        Assert.Equal("AAAA", createdEntry.Secret);

        var updated = new OtpEntry(id, "GitHub", "BBBB", "john.doe");
        Assert.True((await sut.UpdateAsync(updated)).IsSuccess);

        var afterUpdate = await sut.GetAllAsync();
        Assert.True(afterUpdate.IsSuccess);
        var updatedEntry = Assert.Single(afterUpdate.Value);
        Assert.Equal("BBBB", updatedEntry.Secret);
        Assert.Equal("john.doe", updatedEntry.AccountName);

        Assert.True((await sut.DeleteAsync(updated)).IsSuccess);

        var afterDelete = await sut.GetAllAsync();
        Assert.True(afterDelete.IsSuccess);
        Assert.Empty(afterDelete.Value);
    }

    [Fact]
    public async Task ExportEncryptedAsync_WritesDecryptableBlob()
    {
        using var temp = new TempDir();
        var storagePath = Path.Combine(temp.Path, "master.totp");
        var exportPath = Path.Combine(temp.Path, "export.totp");
        var vault = new EchoVaultService();
        var sut = CreateSut(storagePath, vault);

        var entry = new OtpEntry(Guid.NewGuid(), "Google", "CCCC", "a@b.com");
        Assert.True((await sut.AddNewAsync(entry)).IsSuccess);

        var exportResult = await sut.ExportEncryptedAsync(exportPath);

        Assert.True(exportResult.IsSuccess);
        Assert.True(File.Exists(exportPath));
        var exportedBlob = await File.ReadAllBytesAsync(exportPath);
        var exportedItems = vault.DecryptVault(exportedBlob);
        var exported = Assert.Single(exportedItems);
        Assert.Equal(entry.ID, exported.ID);
        Assert.Equal("CCCC", exported.Secret);
    }

    [Fact]
    public async Task ExportEncryptedAsync_WhenTargetDirectoryMissing_ReturnsMappedError()
    {
        using var temp = new TempDir();
        var storagePath = Path.Combine(temp.Path, "master.totp");
        var sut = CreateSut(storagePath, new EchoVaultService());
        Assert.True((await sut.AddNewAsync(new OtpEntry(Guid.NewGuid(), "GitLab", "DDDD"))).IsSuccess);

        var target = Path.Combine(temp.Path, "missing", "export.totp");

        var result = await sut.ExportEncryptedAsync(target);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorCode.ExportFileWriteFailed, result.GetErrorCode());
    }

    [Fact]
    public async Task ReEncryptStorageAsync_PreservesStoredEntries()
    {
        using var temp = new TempDir();
        var storagePath = Path.Combine(temp.Path, "master.totp");
        var sut = CreateSut(storagePath, new EchoVaultService());

        var entry = new OtpEntry(Guid.NewGuid(), "Azure", "EEEE");
        Assert.True((await sut.AddNewAsync(entry)).IsSuccess);

        var reEncryptResult = await sut.ReEncryptStorageAsync();
        var after = await sut.GetAllAsync();

        Assert.True(reEncryptResult.IsSuccess);
        Assert.True(after.IsSuccess);
        Assert.Single(after.Value);
        Assert.Equal(entry.ID, after.Value[0].ID);
    }

    [Fact]
    public async Task BackupOtpEntriesStorageFileAsync_WhenStorageMissing_ReturnsSuccess()
    {
        using var temp = new TempDir();
        var storagePath = Path.Combine(temp.Path, "master.totp");
        var sut = CreateSut(storagePath, new EchoVaultService());

        var result = await sut.BackupOtpEntriesStorageFileAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(Directory.GetFiles(temp.Path, "*.bak*"));
    }

    [Fact]
    public async Task BackupOtpEntriesStorageFileAsync_RotatesBackupsAndSkipsWhenUnchanged()
    {
        using var temp = new TempDir();
        var storagePath = Path.Combine(temp.Path, "master.totp");
        var sut = CreateSut(storagePath, new EchoVaultService());

        Assert.True((await sut.AddNewAsync(new OtpEntry(Guid.NewGuid(), "One", "1111"))).IsSuccess);
        Assert.True((await sut.BackupOtpEntriesStorageFileAsync()).IsSuccess);
        var bak1 = storagePath + ".bak1";
        Assert.True(File.Exists(bak1));
        var firstWrite = File.GetLastWriteTimeUtc(bak1);

        Assert.True((await sut.BackupOtpEntriesStorageFileAsync()).IsSuccess);
        var secondWrite = File.GetLastWriteTimeUtc(bak1);
        Assert.Equal(firstWrite, secondWrite);

        Assert.True((await sut.AddNewAsync(new OtpEntry(Guid.NewGuid(), "Two", "2222"))).IsSuccess);
        Assert.True((await sut.BackupOtpEntriesStorageFileAsync()).IsSuccess);

        Assert.True(File.Exists(storagePath + ".bak1"));
        Assert.True(File.Exists(storagePath + ".bak2"));
    }

    [Fact]
    public async Task GetAllAsync_WhenVaultThrowsCryptographicException_ReturnsDecryptFailedError()
    {
        using var temp = new TempDir();
        var storagePath = Path.Combine(temp.Path, "master.totp");
        await File.WriteAllBytesAsync(storagePath, Encoding.UTF8.GetBytes("blob"));
        var sut = CreateSut(storagePath, new ThrowingVaultService(new CryptographicException("bad")));

        var result = await sut.GetAllAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorCode.OtpStorageDecryptFailed, result.GetErrorCode());
    }

    private static OtpDAL CreateSut(string storagePath, IVaultService vault) =>
        new(NullLogger<OtpDAL>.Instance, vault, storagePath);

    private sealed class EchoVaultService : IVaultService
    {
        public List<OtpEntry> DecryptVault(byte[] encryptedBlob) =>
            System.Text.Json.JsonSerializer.Deserialize<List<OtpEntry>>(encryptedBlob) ?? [];

        public byte[] EncryptVault(List<OtpEntry> entries) =>
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(entries);
    }

    private sealed class ThrowingVaultService(Exception exception) : IVaultService
    {
        public List<OtpEntry> DecryptVault(byte[] encryptedBlob) => throw exception;
        public byte[] EncryptVault(List<OtpEntry> entries) => throw exception;
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
