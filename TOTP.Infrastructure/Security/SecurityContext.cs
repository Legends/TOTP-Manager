using System;
using System.Runtime.InteropServices;
using TOTP.Core.Security.Interfaces;

namespace TOTP.Core.Security.Services;

public sealed class SecurityContext : ISecurityContext, IDisposable
{
    private byte[]? _rawDek;
    private GCHandle _memoryHandle;

    public bool IsUnlocked => _rawDek != null;

    /// <summary>
    /// Securely stores the Data Encryption Key in RAM.
    /// Uses Memory Pinning to prevent the GC from leaving plain-text copies in RAM.
    /// </summary>
    public void SetDek(byte[] dek)
    {
        if (dek == null || dek.Length == 0) return;

        // 1. Clean up any existing key before setting a new one
        Lock();

        // 2. Clone and Pin the memory
        // Pinned memory prevents the Garbage Collector from moving the array,
        // ensuring that when we call Array.Clear, we are clearing the ONLY copy.
        _rawDek = (byte[])dek.Clone();
        _memoryHandle = GCHandle.Alloc(_rawDek, GCHandleType.Pinned);
    }

    /// <summary>
    /// Returns the active DEK. 
    /// Note: The caller should use this key immediately and NOT store it in a long-lived field.
    /// </summary>
    public byte[] GetDek()
    {
        return _rawDek ?? throw new InvalidOperationException("Vault is locked. Access denied.");
    }

    /// <summary>
    /// Securely wipes the key from memory and unpins the handle.
    /// </summary>
    public void Lock()
    {
        if (_rawDek != null)
        {
            // 1. Zero-out the memory contents
            Array.Clear(_rawDek, 0, _rawDek.Length);

            // 2. Release the handle so the GC can reclaim the empty array
            if (_memoryHandle.IsAllocated)
            {
                _memoryHandle.Free();
            }

            _rawDek = null;
        }
    }

    public void Dispose()
    {
        Lock();
    }
}