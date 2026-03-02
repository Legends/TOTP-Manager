using TOTP.Core.Enums;

namespace TOTP.Security.Models;

/// <summary>
/// Represents the persisted authorization configuration.
/// This acts as both the "Lifeboat" (Password) and "Fast-Track" (Hello/TPM).
/// </summary>
public sealed class AuthorizationProfile
{
    // The current preferred method of entry
    public AuthorizationGateKind Gate { get; set; } = AuthorizationGateKind.None;

    // --- Lifeboat Path (Master Password / Argon2id) ---
    public byte[]? PasswordSalt { get; set; }
    public int ArgonIterations { get; set; }
    public int ArgonMemorySize { get; set; }
    public byte[]? PasswordWrappedDek { get; set; }
    public byte[]? DekNonce { get; set; }

    // --- Fast-Track Path (Windows Hello / TPM) ---
    public byte[]? HelloWrappedDek { get; set; }
    public string? HelloKeyId { get; set; }

    // --- Computed Logic for UI & Services ---

    /// <summary>
    /// Returns true if any authorization method has been set up.
    /// </summary>
    public bool IsConfigured => Gate != AuthorizationGateKind.None;

    /// <summary>
    /// Returns true if the Master Password recovery path is ready.
    /// </summary>
    public bool IsPasswordSetup => PasswordWrappedDek != null && PasswordSalt != null;

    /// <summary>
    /// Returns true if the Windows Hello / TPM path is linked to this hardware.
    /// This fixes the CS1061 error in your SettingsViewModel.
    /// </summary>
    public bool HasHelloSetup => HelloWrappedDek != null && !string.IsNullOrEmpty(HelloKeyId);
}