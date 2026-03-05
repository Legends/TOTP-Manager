using TOTP.Core.Security.Models;
using TOTP.Infrastructure.Services;

namespace TOTP.Tests.Services;

public sealed class PasswordHasherTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Hash_WhenPasswordBlank_ThrowsArgumentException(string password)
    {
        Assert.Throws<ArgumentException>(() => PasswordHasher.Hash(password));
    }

    [Fact]
    public void Hash_WhenPasswordValid_ReturnsPopulatedRecord()
    {
        var record = PasswordHasher.Hash("my-secure-password");

        Assert.NotNull(record.Salt);
        Assert.NotNull(record.Hash);
        Assert.Equal(16, record.Salt.Length);
        Assert.Equal(32, record.Hash.Length);
        Assert.True(record.Iterations > 0);
        Assert.True(record.MemorySize > 0);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue_AndWrongReturnsFalse()
    {
        var record = PasswordHasher.Hash("correct-password");

        Assert.True(PasswordHasher.Verify("correct-password", record));
        Assert.False(PasswordHasher.Verify("wrong-password", record));
    }

    [Fact]
    public void Verify_WhenRecordInvalid_ReturnsFalse()
    {
        var invalidRecord = new PasswordRecord(null!, null!, 4, 1024);

        Assert.False(PasswordHasher.Verify("pw", invalidRecord));
    }

    [Fact]
    public void HashWithParams_WithSamePasswordAndRecord_IsDeterministic()
    {
        var record = PasswordHasher.Hash("repeatable");

        var left = PasswordHasher.HashWithParams("repeatable", record);
        var right = PasswordHasher.HashWithParams("repeatable", record);
        var wrong = PasswordHasher.HashWithParams("different", record);

        Assert.Equal(32, left.Length);
        Assert.True(System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(left, right));
        Assert.False(System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(left, wrong));
    }
}
