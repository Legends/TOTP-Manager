# Authorization envelope version 2

## Status and scope

Version 2 is the portable authorization wire contract for the first public release. The application has not been published and has no existing users, so development-era settings formats carry no compatibility commitment. Historical DPAPI fixtures remain design and regression evidence only; shipping code does not need a legacy migration reader.

This step defines the envelope header and mandatory password-recovery wrapper. It does not implement file I/O, atomic replacement, or platform quick unlock. Those remain separate M2 tasks.

## Security invariants

- The envelope never contains a plaintext DEK, master password, password-derived key, OTP seed, or vault plaintext.
- A password wrapper is mandatory. A platform quick-unlock mechanism is optional convenience and must never become the only recovery path.
- Readers must fail closed on an unknown format, envelope version, KDF algorithm/version, or key-wrap algorithm.
- Readers must validate all lengths and KDF resource bounds before invoking Argon2id.
- The envelope and its bounded previous-version backups require platform-appropriate current-user file protection even though the DEK is wrapped.
- Envelope contents and Base64 fields must not be logged.

## Encoding

The v2 payload is UTF-8 JSON with explicit camel-case wire names. Byte arrays use standard padded Base64. Property order and insignificant whitespace are not part of the contract.

The proposed file name remains `authorization-envelope.bin` to discourage casual editing; the extension is not an encryption claim. The security boundary is the authenticated password wrapper plus filesystem access controls, not obscurity of the encoding.

```json
{
  "format": "totp-authorization-envelope",
  "version": 2,
  "passwordWrapper": {
    "kdf": {
      "algorithm": "argon2id",
      "version": 19,
      "salt": "<Base64>",
      "passes": 3,
      "memoryKiB": 65536,
      "parallelism": 1
    },
    "wrappedKey": {
      "algorithm": "aes-256-gcm",
      "nonce": "<Base64>",
      "ciphertext": "<Base64 ciphertext with appended tag>"
    }
  },
  "quickUnlockWrapper": {
    "provider": "windows-hello-tpm",
    "providerVersion": 1,
    "authenticationPolicy": "user-verification-required",
    "keyReference": "<opaque platform key reference>",
    "wrappedKey": {
      "algorithm": "rsa-oaep-sha256",
      "ciphertext": "<Base64 RSA-2048 ciphertext>"
    }
  }
}
```

## Field contract

| Field | Contract |
| --- | --- |
| `format` | Exact ASCII identifier `totp-authorization-envelope` |
| `version` | Integer `2` |
| `passwordWrapper` | Required recovery wrapper |
| `kdf.algorithm` | Exact identifier `argon2id` |
| `kdf.version` | Argon2 version `19` (`0x13`) |
| `kdf.salt` | Per-wrapper random salt; current implementation uses 16 bytes |
| `kdf.passes` | Argon2id pass count |
| `kdf.memoryKiB` | Argon2id memory cost in KiB, never bytes |
| `kdf.parallelism` | Argon2id degree of parallelism; persisted instead of relying on an implementation default |
| `wrappedKey.algorithm` | Exact identifier `aes-256-gcm` |
| `wrappedKey.nonce` | Per-wrap nonce; 12 bytes for AES-GCM |
| `wrappedKey.ciphertext` | Wrapped 32-byte vault DEK with the 16-byte authentication tag appended |

The legacy implementation uses empty associated data. Because no released data must remain compatible, the v2 password wrapper instead uses the exact UTF-8 bytes of `totp-manager/authorization-envelope/v2/password-wrapper` as AES-GCM associated data. This domain-separates the wrapped key from other AES-GCM uses. A reader must not retry with empty associated data after v2 authentication fails.

The implemented v2 reader accepts 3-10 passes, 65,536-262,144 KiB of memory, and parallelism exactly 1. Salt, nonce, and ciphertext lengths must be exactly 16, 12, and 48 bytes respectively. These v2 minimums intentionally reject the weaker implementation-minimum values retained only by the development-era legacy API. Parallelism remains explicit in the wire format so another implementation cannot silently choose a different value; the current NSec backend supports one for this construction.

## Implementation status

`IMasterPasswordService.WrapKeyV2Async` creates a complete `PasswordKeyWrapperV2`, including the algorithm/version identifiers, salt, passes, memory cost in KiB, parallelism, nonce, and authenticated ciphertext. `UnwrapKeyV2Async` consumes those persisted values and validates the complete wrapper before starting Argon2id.

The existing tuple-based methods remain temporarily for the running WPF authorization flow and synthetic historical-fixture tests. They use the development-era empty-AAD construction and are not a fallback for v2. Envelope file persistence and activation are later M2 steps.

## Platform quick-unlock metadata

`quickUnlockWrapper` is optional and device-local. Its absence never makes the envelope invalid, while its presence never relaxes the requirement for a valid `passwordWrapper`. The preferred unlock UI choice remains a preference rather than cryptographic metadata.

| Field | Contract |
| --- | --- |
| `provider` | Reviewed platform adapter identifier; currently `windows-hello-tpm` |
| `providerVersion` | Provider contract version; currently `1` for Windows |
| `authenticationPolicy` | Must be `user-verification-required`; silent access is not a supported policy |
| `keyReference` | Opaque, non-secret reference to platform-managed key material; never the key itself |
| `wrappedKey.algorithm` | Provider-approved key-wrap algorithm |
| `wrappedKey.nonce` | Optional algorithm-specific nonce; omitted for the Windows RSA wrapper |
| `wrappedKey.ciphertext` | Platform-wrapped 32-byte vault DEK |

The Windows provider maps the current CNG key name to `keyReference` and the RSA-OAEP-SHA256 result to `wrappedKey.ciphertext`. Version 1 requires the Microsoft Platform Crypto Provider behavior, RSA-2048 ciphertext of exactly 256 bytes, no nonce, and an explicit Windows Hello verification before private-key use. A software-key or silent-decrypt fallback does not satisfy this provider identifier.

`PlatformQuickUnlockContract.IsSupported` is the fail-closed registry for metadata understood today. Unknown provider identifiers, provider versions, authentication policies, algorithms, unexpected nonces, malformed key references, and invalid ciphertext lengths must be treated as unavailable quick unlock. The application then requires the master password; it must not reinterpret the metadata or try a weaker provider.

When a platform key is missing, reset, or belongs to another device, the password wrapper remains authoritative. After password unlock, the stale quick-unlock wrapper may be removed and a new device-local wrapper registered. Key references and wrapped ciphertext are excluded from logs even though the reference is not secret and the DEK remains encrypted.

## Platform secret-store contract

`IPlatformSecretStore` defines the portable boundary for device-local secret storage used by future quick-unlock adapters. It exposes a stable provider identifier, an explicit availability state, and asynchronous store, retrieve, and delete operations with cancellation. This contract does not itself enable quick unlock and has no WPF or operating-system dependency.

Contract semantics:

- `StoreAsync` creates or replaces one application-owned reference and succeeds only after the platform store has accepted its own copy.
- `RetrieveAsync` returns a caller-owned `SensitiveBuffer`; a successful null value means the reference is absent or was reset.
- `DeleteAsync` is idempotent, including when the reference is already absent.
- Expected failures use `PlatformSecretStoreError` and `PlatformSecretStoreErrorCode`; cancellation propagates as cancellation rather than being converted to a failure result.
- `PlatformSecretStoreAvailability.Unknown` is the zero/default state. Callers enable store-dependent behavior only for explicit `Available`.
- Providers reject empty/invalid references, empty secrets, unsupported sizes, and access-control failures without logging the reference together with secret data.

`SensitiveBuffer` copies retrieved bytes, exposes them as read-only memory, and zeroes its owned array on disposal. The caller owns and must dispose it promptly. Providers must likewise clear temporary copies where practical and must not retain caller memory after `StoreAsync` completes.

The secret store is intended to hold a random device-local quick-unlock wrapping secret, never the master password, OTP seeds, or the sole copy of the vault DEK. An unavailable, missing, denied, or corrupt store routes to master-password recovery; there is no plaintext filesystem fallback. Concrete Windows, macOS, and Linux providers and their platform access-control policies remain later implementation steps.

## Platform quick-unlock contract

`IPlatformQuickUnlock` is the portable user-verification and key-wrapper boundary. It exposes provider identity, availability, registration, unlock attempts, and idempotent removal without exposing WPF windows, Windows Hello types, CNG handles, Keychain APIs, or Secret Service APIs.

Contract semantics:

- `RegisterAsync` accepts a 32-byte vault key only after the orchestration layer has verified that master-password recovery is ready. It performs required platform verification and returns complete `PlatformQuickUnlockWrapperV2` metadata.
- The returned wrapper provider must equal the adapter `ProviderId` and pass the reviewed provider validation before it can become active.
- `TryUnlockAsync` validates metadata before platform access and returns `PlatformQuickUnlockAttempt` for expected outcomes.
- A successful attempt always owns a non-null `SensitiveBuffer` containing the recovered vault key. The caller must dispose the attempt promptly after copying the key into the active security context.
- Cancelled, unavailable, not-configured, policy-disabled, retries-exhausted, verification-failed, and key-not-found attempts never contain key material and route to password recovery as appropriate.
- Unexpected adapter failures use `PlatformQuickUnlockError`; registration-specific cancellation or policy outcomes use its typed error codes because no wrapper exists to return with an attempt.
- `RemoveAsync` is idempotent for missing/reset platform keys. The orchestration layer must authorize removal before calling it.
- Application cancellation propagates as cancellation. Platform prompt cancellation remains an explicit expected outcome rather than being confused with application cancellation.
- `PlatformQuickUnlockAvailability.Unknown` is fail-closed and never enables registration or automatic quick unlock.

An implementation may depend on `IPlatformSecretStore`, a non-exportable platform key, or both. It must never silently substitute software-only storage for metadata that claims hardware/user-verification guarantees. The legacy `IHelloGate` remains temporarily for the unpublished WPF flow and will be adapted or retired in the later Windows contract-preservation step.

## Reader rules

A v2 reader must reject the payload before key derivation when:

- required fields are missing, null, duplicated, or represented with the wrong JSON type;
- `format`, envelope `version`, or either algorithm identifier is unsupported;
- Argon2 version is not 19;
- numeric values are outside implementation security bounds;
- Base64 is invalid or decoded lengths are invalid;
- trailing non-whitespace content exists;
- the AES-GCM authentication check fails.

Unsupported or malformed optional quick-unlock metadata does not invalidate a sound password wrapper. It disables quick unlock and routes the user to master-password recovery. A parser error in the envelope itself still rejects the entire file.

Unknown non-critical properties may be retained for forward-compatible metadata only after a deliberate reader policy is implemented. They must not override known fields or weaken validation.

## Data flow

```text
master password (memory only)
  + persisted Argon2id parameters
  -> password-derived KEK (memory only)
  + AES-256-GCM wrappedKey
  -> vault DEK (memory only)
  -> existing encrypted vault verification
```

Temporary password bytes, KEK material, and DEK copies must be cleared or disposed as soon as practical. The envelope may be committed only after the recovered DEK successfully opens the existing vault.

## Compatibility and migration impact

- Version 2 does not reuse the ambiguous legacy `AppSettings`/`AuthorizationProfile` root shape.
- Preferences and the preferred unlock UI choice do not belong in the password wrapper.
- The current Argon2id/AES-GCM construction remains the baseline, but v2 persists every KDF parameter and adds fixed associated data. Development data may be reset or reconfigured rather than migrated byte-for-byte.
- Shipping startup reads v2 only and does not probe historical JSON shapes. The development-era DPAPI fixtures are retained solely to document why the ambiguous format must not return.
- Platform quick-unlock metadata is optional, explicitly typed, and validated against a reviewed provider registry.

## Threat review

The clean version discriminator removes the legacy type-confusion risk. Explicit algorithm identifiers and KDF units reduce cross-platform interpretation errors. Persisting parallelism prevents a hidden implementation default from changing derived keys. Fixed associated data domain-separates the password wrapper without a legacy fallback. Remaining risks are denial of service from hostile KDF values, file replacement/rollback, and metadata tampering; bounded validation, current-user file protection, authenticated unwrap, vault verification, and later atomic-write/backup work address those risks.
