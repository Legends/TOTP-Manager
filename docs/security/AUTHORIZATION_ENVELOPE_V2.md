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

The existing tuple-based methods remain only for the unregistered development-era authorization implementation and synthetic historical-fixture tests. They use the development-era empty-AAD construction and are not a fallback for v2. The active WPF authorization flow uses only the v2 wrapper methods.

`AuthorizationEnvelopeV2Codec` is the strict portable codec. It caps payloads at 64 KiB, limits JSON depth, rejects duplicate and unknown properties, validates the complete password wrapper, and rejects unsupported headers before any KDF work. Unsupported optional quick-unlock metadata may be read so password recovery remains possible, but the writer will not create unsupported metadata.

`AuthorizationEnvelopeStore` is the active authorization store at `authorization-envelope.bin`. It performs bounded streaming reads, clears plaintext payload buffers, and applies platform file protection. Saves write through a same-directory hardened staging file, force staged bytes through intermediate filesystem buffers, decode the staged payload before commit, and use same-volume atomic replacement when an envelope already exists. Replacement preserves exactly one `authorization-envelope.bin.previous` backup; a later save replaces that backup rather than growing an unbounded history. The committed file is decoded again without cancellation, and a failure after the commit point restores the previous envelope (or removes a failed first write). The development-era DPAPI `settings.totp` DAL is no longer registered in the runtime composition.

This storage hardening reduces partial-write, replacement, and unbounded-secret-retention risks but does not prove that a newly wrapped DEK opens the vault. Activation must still unwrap the password wrapper and verify the existing vault before calling the store. The wire format and cryptographic construction are unchanged, and there is no compatibility obligation for development-era data. Integration tests cover first write, replacement, bounded backup rotation, malformed and oversized reads, pre-commit ACL failure, and post-commit rollback.

`IVaultKeyVerifier` provides the side-effect-free cryptographic check needed by that future activation sequence. `VaultService` verifies the vault header, AES-GCM authentication, and decrypted account JSON with a caller-supplied 32-byte candidate DEK without placing that key in `ISecurityContext`. Wrong keys and authenticated-ciphertext tampering intentionally share the same authentication-failed outcome, temporary plaintext is cleared, and caller-owned key and vault buffers are never modified. Missing-vault first-run policy remains the responsibility of activation orchestration; this primitive validates only an existing vault payload.

`IStoredVaultKeyVerifier` adds the read-only persistence boundary around that primitive. It resolves the same configured vault path as `AccountDAL`, applies current-user file protection, rejects payloads above 16 MiB before allocation, clears the bounded ciphertext buffer after use, and reports a missing vault explicitly for first-run orchestration. Access, size, and other I/O failures remain typed failures rather than being confused with a wrong key. It never creates or changes a vault and does not activate an authorization envelope.

`AccountDAL` now commits the active encrypted vault and encrypted exports through write-through same-directory staging. It compares staged and committed ciphertext byte-for-byte with the newly produced encrypted blob without decrypting a second copy of the account data. Replacements retain a temporary rollback file and a SHA-256 digest of the previous ciphertext until post-commit hardening and verification succeed; failed first commits are removed, failed replacements restore the exact previous bytes, and temporary encrypted output and digest buffers are cleared. Backup rotation is serialized with vault writes and copies generations forward from oldest to newest through the same verified commit helper. Sources are never destructively moved, exactly five generations are retained, and `bak1` is published only after every older generation succeeds. An interruption may leave a harmless duplicate older generation but cannot expose a partial backup or publish a new latest backup with an incomplete rotation. Deterministic tests cover truncated staging plus pre-commit, post-commit, and mid-rotation failures.

`IAuthorizationEnvelopeActivator` is the active boundary for turning a proposed v2 envelope into the persisted envelope. It serializes and decodes the proposal through the strict codec, unwraps that exact wire-equivalent password wrapper, fixed-time compares the recovered key with an owned copy of the candidate DEK, and clears the owned key, serialized payload, and decoded envelope arrays. An existing vault must authenticate and deserialize with the candidate key before persistence; on first run, the explicit missing-vault outcome is accepted only after the password wrapper proves it contains the candidate key. Cancellation is checked before the atomic store call, and typed verification or persistence failures prevent activation.

`IAuthorizationEnvelopePasswordLifecycle` owns the active v2 first-run password configuration and recovery-password replacement. Initial configuration refuses to replace an existing envelope, generates a fresh 32-byte DEK, creates a v2 password wrapper, and delegates persistence only to the verified activator. Replacement requires the current recovery password, unwraps the existing DEK, creates a new password wrapper, preserves the optional reviewed quick-unlock wrapper unchanged, and again delegates verification and atomic persistence to the activator. Both operations return an independently owned `SensitiveBuffer` only after successful activation so the authorization facade can explicitly activate the in-memory security context. Generated, recovered, loaded-envelope, proposed-wrapper, and cancellation-path buffers are cleared at their ownership boundaries.

`AppPreferencesV1` defines the non-secret half of that separation. Its `preferences.json` wire format contains only UI/runtime preferences, uses explicit format/version fields and string enum names, and persists idle timeout in whole minutes. The optional interface-scale percentage is non-secret presentation metadata: zero means use the platform scale unchanged, while reviewed 25-percent increments from 100 through 300 are accessibility multipliers. The preferred unlock method is either `Password` or the platform-neutral `PlatformQuickUnlock`; it is a UX choice only and never proves that a quick-unlock wrapper exists or is usable. Password is the default and fail-closed fallback. The codec rejects duplicate or unknown fields, numeric enum values, invalid cultures, and values outside the UI-supported ranges. Because the model has no authorization member and unknown fields are disallowed, password salts, wrapped keys, platform references, and OTP material cannot be serialized through this contract.

`AppPreferencesStore` is the active non-secret preference store. Plaintext JSON is acceptable for these reviewed preference fields, while current-user file protection still limits casual cross-user access. Reads are bounded to 32 KiB and their buffers are cleared defensively. Writes use a same-directory write-through staging file, decode the staged bytes before commit, and revalidate the committed file. Replacement retains a temporary rollback copy until validation succeeds; a failed first commit is removed, and a failed replacement restores the previous preferences. Deterministic tests cover truncated staging data plus pre-commit and post-commit hardening failures. This is not permission to add authorization metadata or other sensitive values to the preferences model; any new field requires a data-classification review.

`AppPreferencesMapper` is the sole allowlisted bridge between mutable in-memory `IAppSettings` and `AppPreferencesV1`. It copies only reviewed preference fields, normalizes legacy/out-of-range values to codec-valid values, and never reads or replaces the `AuthorizationProfile`. This makes authorization exclusion behavioral rather than relying only on JSON naming. Coordinated service activation remains pending.

`PortableSettingsService` is the active `ISettingsService` implementation backed only by `IAppPreferencesStore`. It maintains the stable mutable settings object expected by the current WPF consumers, applies portable preferences on first load, retries failed loads, and maps only allowlisted preference fields on save. Tests populate the in-memory legacy authorization member with synthetic password-wrapper data and verify that the persisted `AppPreferencesV1` contract has no authorization, password, or wrapped-key property.

The Avalonia desktop host reads the same bounded, validated preferences contract before platform initialization so a saved interface-scale multiplier can affect every top-level window consistently. System scaling remains the default, explicit process-level Avalonia overrides retain precedence, malformed or unavailable preferences fail back to system scaling, and no display setting changes authorization, encrypted storage, clipboard policy, or secret data flow. Adding the optional field is backward compatible with existing version-1 preference files because an absent value defaults to system scaling.

`PortableAuthorizationService` is the active WPF-facing facade. It implements the existing authorization API using only portable preferences, the v2 session, the v2 password lifecycle, quick-unlock enrollment, and the platform adapter. Initialization derives configured state from envelope presence and projects a platform preference to the password gate when no reviewed wrapper is usable. Startup prompts the platform only when preference and wrapper agree. Password and platform unlock delegate all key recovery and vault verification to the session. Setup and password replacement activate only owned lifecycle result buffers after session refresh, clear the temporary security-context input, and then update presentation state. Quick-unlock enrollment requires the explicit recovery-password API; the parameterless compatibility method fails closed with `PasswordRequired`. Preference-save failures restore the prior in-memory choice, and a runtime quick-unlock failure can project the password gate without silently rewriting the persisted preference.

The Avalonia desktop composition root activates this same portable infrastructure graph. Windows registers the reviewed Hello/TPM adapter. macOS registers the reviewed data-protection Keychain/user-presence adapter. Linux registers Secret Service as an independently reported platform store but deliberately supplies `UnavailablePlatformQuickUnlock`, preserving the approved master-password-only policy. A wrapper from another device or operating system has no matching provider and routes safely to mandatory master-password recovery.

`AvaloniaStartupCoordinator` is the Avalonia initialization boundary. It loads only the allowlisted preferences contract before initializing authorization. Password-preferred sessions project password setup or password unlock without an automatic credential operation. When a verified envelope contains reviewed quick-unlock metadata and preferences select it, startup delegates to the authorization facade; the shell opens only if the result is successful and shared authorization state is actually unlocked. Every unavailable, cancelled, reset-key, policy, retry, or inconsistent-success outcome projects the master-password recovery gate. Preference failures and unexpected boundary exceptions produce a retryable, sanitized UI state that explicitly confirms encrypted data was not changed. Exception messages are not logged at this boundary because they may contain paths or user-controlled data. Cancellation propagates, and startup performs no direct envelope or vault write, so this slice adds no storage-format or migration impact.

The Avalonia password-unlock slice delegates exclusively to `IAuthorizationService.TryUnlockWithPasswordAsync`; it does not read the envelope or vault from presentation code. The bound password is copied only to satisfy the existing string-based authorization contract and is removed from the view model before the asynchronous verification completes. Managed strings cannot be deterministically zeroed, so replacing that cross-layer contract with an owned clearable credential buffer remains future hardening work. Success is derived from the authorization result, while rejection and unexpected exceptions use generic UI text and never echo or log the supplied password or exception message. This slice changes no authorization data flow, cryptography, or persisted format.

First-run setup preserves a typed existing-vault conflict when a candidate v2 key cannot verify a vault already present on disk. The WPF setup view reports that the existing encrypted data was not changed and does not misclassify the refusal as a password-policy error. The application never deletes or replaces the conflicting vault automatically; starting fresh requires an explicit operator action to move the development-era vault aside, while recovery requires its matching historical authorization material.

The WPF unlock and settings workflows obtain gate and configuration status from `IAuthorizationService.State` instead of reading `AuthorizationProfile`. Enabling platform quick unlock and replacing the recovery password require an explicit current-password prompt; cancellation fails closed before enrollment or password lifecycle work. The prompt does not retain or persist the supplied password. This removes the presentation dependency on development-era authorization metadata while preserving the current facade projection.

`AuthorizationState.SetConfiguration` now derives configured state from envelope presence and the portable preferred-unlock preference rather than requiring an `AuthorizationProfile`. The current WPF UI still consumes `AuthorizationGateKind`, so the state temporarily projects `PlatformQuickUnlock` to the legacy `Hello` gate name. Missing configuration and invalid preference values fail closed to password setup/password unlock. `SetProfile` remains only for the unregistered development-era implementation and its tests and can now be removed in cleanup.

`IAuthorizationEnvelopeSession` is the active v2 load and unlock path. Initialization loads one strictly decoded envelope and reports configured and supported-quick-unlock capability without exposing the cached wrapper. Malformed-envelope errors remain typed load failures and leave the session uninitialized. Password unlock uses only `UnwrapKeyV2Async`, and wrong passwords cannot reach vault verification or the security context. Platform unlock selects a registered adapter by the wrapper's exact reviewed provider identifier; absent adapters, unsupported wrappers, unavailable platform state, missing platform keys, and unconfigured platform state all require the password recovery path. A combined reset-key regression verifies that `KeyNotFound` falls back to a successful password-wrapper recovery and vault-verified unlock in the same session. Both unlock methods verify the recovered DEK against the existing vault (or the explicit first-run no-vault state) and set `ISecurityContext` only after verification. Adapter failures and vault failures remain separately typed, while user cancellation and application cancellation remain distinct. Recovered keys, the temporary array passed to the synchronously copying security context, and cached envelope arrays are cleared or disposed at their ownership boundaries.

## Platform quick-unlock metadata

`quickUnlockWrapper` is optional and device-local. Its absence never makes the envelope invalid, while its presence never relaxes the requirement for a valid `passwordWrapper`. The preferred unlock UI choice remains a preference rather than cryptographic metadata.

| Field | Contract |
| --- | --- |
| `provider` | Reviewed platform adapter identifier: `windows-hello-tpm` or `macos-keychain-user-presence` |
| `providerVersion` | Provider contract version; currently `1` for both reviewed providers |
| `authenticationPolicy` | Must be `user-verification-required`; silent access is not a supported policy |
| `keyReference` | Opaque, non-secret reference to platform-managed key material; never the key itself |
| `wrappedKey.algorithm` | Provider-approved key-wrap algorithm |
| `wrappedKey.nonce` | Optional algorithm-specific nonce; omitted by both current providers |
| `wrappedKey.ciphertext` | Provider-specific bounded metadata: Windows RSA ciphertext or the macOS opaque-reference binding |

The Windows provider maps the current CNG key name to `keyReference` and the RSA-OAEP-SHA256 result to `wrappedKey.ciphertext`. Version 1 requires the Microsoft Platform Crypto Provider behavior, RSA-2048 ciphertext of exactly 256 bytes, no nonce, and an explicit Windows Hello verification before private-key use. A software-key or silent-decrypt fallback does not satisfy this provider identifier.

The macOS provider stores the 32-byte DEK in a data-protection Keychain generic-password item whose access control requires user presence. `keyReference` is the opaque account identifier and `wrappedKey.ciphertext` is a fixed 32-byte SHA-256 binding of that non-secret reference, not encrypted key material. Retrieval is authorized by the system through LocalAuthentication/Keychain policy. Copying the envelope does not copy the item, and modifying the reference without its binding is rejected before platform access.

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

`SecurityContext` accepts only an exact 32-byte DEK, clones caller-owned input into one pinned owned buffer, returns only independent caller-owned copies, and cryptographically zeroes the pinned buffer before unpinning it on lock or disposal. Invalid key lengths fail before changing the active context. Focused ownership tests retain the internal array across `Lock` to verify it is zeroed and verify that clearing or modifying caller copies cannot alter the active key. The active v2 lifecycle, session, activator, platform enrollment, Windows quick-unlock adapter, and facade each clear their temporary DEK copies at their established ownership boundaries.

The secret store never holds the master password or OTP seeds. A provider may hold a random wrapping secret, or the device-local copy of the vault DEK only when the reviewed operating-system store enforces user presence and the independent password wrapper remains the authoritative recovery path. An unavailable, missing, denied, or corrupt store routes to master-password recovery; there is no plaintext filesystem fallback. macOS uses the data-protection Keychain for reviewed quick unlock. Linux integrates Secret Service without enabling quick unlock and transfers binary data through clearable standard-input/output buffers rather than process arguments.

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

An implementation may depend on `IPlatformSecretStore`, a non-exportable platform key, or both. It must never silently substitute software-only storage for metadata that claims hardware/user-verification guarantees. `IHelloGate` remains temporarily as the low-level Windows verification seam used by the portable adapter and can be retired in a later provider cleanup.

`WindowsPlatformQuickUnlock` now adapts the existing Hello/TPM implementation to this contract. Registration and unlock require an explicit `UserConsentVerifier` success before TPM key use. Registration creates a non-exportable RSA-2048 key with the Microsoft Platform Crypto Provider, emits only the reviewed `windows-hello-tpm`/RSA-OAEP-SHA256 metadata, and removes incomplete registrations on failure. Unlock validates all metadata before prompting, copies a recovered 32-byte DEK into `SensitiveBuffer`, and clears the provider-returned array. Removal is idempotent for missing keys. The adapter never selects the Microsoft Software Key Storage Provider and has no silent-decrypt fallback.

`MacOSPlatformQuickUnlock` uses `MacOSKeychainSecretStore` with Security.framework and LocalAuthentication. The store targets `kSecUseDataProtectionKeychain`, guards the item with `SecAccessControlCreateFlags.userPresence`, performs blocking Keychain access off the UI thread, bounds native results, and clears all temporary secret arrays. Registration emits only the reviewed macOS metadata. Retrieval cancellation, missing items, unavailable authentication, reset Keychain state, and invalid key length fail closed to recovery.

`IPlatformQuickUnlockEnrollment` is the active portable enablement boundary. Its implementation reloads the active envelope and refuses enrollment unless the supplied recovery password unwraps a 32-byte DEK that verifies the existing vault. Only then does it check platform availability and request registration. It validates successful adapter output against both the selected provider identity and the reviewed metadata registry before atomically saving the updated envelope. Existing quick-unlock metadata is never silently replaced. A failed or cancelled save removes the new platform registration, and all loaded envelope and recovered-key buffers are cleared.

## Reader rules

A v2 reader must reject the payload before key derivation when:

- required fields are missing, null, duplicated, or represented with the wrong JSON type;
- `format`, envelope `version`, or either algorithm identifier is unsupported;
- Argon2 version is not 19;
- numeric values are outside implementation security bounds;
- Base64 is invalid or decoded lengths are invalid;
- trailing non-whitespace content exists;
- the AES-GCM authentication check fails.

The implemented codec performs structural and parameter validation. AES-GCM authentication occurs only when `IMasterPasswordService.UnwrapKeyV2Async` is called with the user-supplied password.

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

Windows Hello verification
  + persisted platform key reference and RSA-OAEP-SHA256 ciphertext
  -> non-exportable RSA-2048 private-key operation in the TPM provider
  -> vault DEK (short-lived provider array, then owned SensitiveBuffer)
  -> existing encrypted vault verification

enable platform quick unlock
  -> reload active v2 envelope
  + recovery password -> unwrap 32-byte vault DEK
  -> verify existing vault without side effects
  -> verify platform availability and register
  -> validate reviewed provider metadata
  -> atomically persist envelope with optional quick-unlock wrapper
```

Temporary password bytes, KEK material, and DEK copies must be cleared or disposed as soon as practical. The envelope may be committed only after the recovered DEK successfully opens the existing vault.

## Compatibility and migration impact

- Version 2 does not reuse the ambiguous legacy `AppSettings`/`AuthorizationProfile` root shape.
- Preferences and the preferred unlock UI choice do not belong in the password wrapper.
- The current Argon2id/AES-GCM construction remains the baseline, but v2 persists every KDF parameter and adds fixed associated data. Development data may be reset or reconfigured rather than migrated byte-for-byte.
- Shipping startup reads v2 only and does not probe historical JSON shapes. The development-era DPAPI fixtures are retained solely to document why the ambiguous format must not return.
- Platform quick-unlock metadata is optional, explicitly typed, and validated against a reviewed provider registry.
- Enrollment changes only the optional quick-unlock wrapper; the mandatory password wrapper remains intact and independently recoverable.
- The portable authorization facade preserves the current WPF result and gate projection API, so presentation cutover does not require a legacy authorization-format reader.
- The app is not publicly released and has no released-user quick-unlock registrations. This step therefore adds no legacy platform-key migration or compatibility fallback.

## Threat review

The clean version discriminator removes the legacy type-confusion risk. Explicit algorithm identifiers and KDF units reduce cross-platform interpretation errors. Persisting parallelism prevents a hidden implementation default from changing derived keys. Fixed associated data domain-separates the password wrapper without a legacy fallback. Password replacement requires proof of the current recovery password and preserves rather than silently recreating platform metadata. The Windows adapter refuses unreviewed metadata before prompting, requires explicit Hello verification, creates non-exportable TPM-provider keys, clears transient DEK arrays, and removes incomplete registrations. Enrollment cannot reach platform registration until password unwrap and side-effect-free vault verification both succeed, preventing platform quick unlock from becoming the only recovery path. Persistence failure and application cancellation after registration trigger compensating platform-key removal. Password recovery remains required because loss or reset of the platform key makes its wrapper unrecoverable. Remaining risks are denial of service from hostile KDF values, file replacement/rollback, metadata tampering, cleanup failure after a partial enrollment, and loss of TPM/Hello state; bounded validation, current-user file protection, authenticated unwrap, vault verification, typed cleanup failures, password recovery, and atomic-write/backup work address those risks.

## Test evidence

- `WindowsPlatformQuickUnlockTests` covers detailed availability, reviewed metadata emission, verification outcomes, invalid inputs, provider failures, incomplete-key cleanup, recovered-key ownership and clearing, missing platform keys, and fail-closed removal.
- `PlatformQuickUnlockEnrollmentTests` covers mandatory recovery-password proof, vault verification, availability gating, metadata validation, atomic persistence, compensating cleanup, cancellation, and buffer clearing.
- `PortableSettingsServiceTests` covers defaults, portable preference mapping, stable in-memory identity, retry after typed load failure, typed save failure, and exclusion of synthetic authorization material.
- `AuthorizationEnvelopeSessionTests` covers initialization plus password and platform unlock, including provider selection, password fallback, expected platform outcomes, vault verification, typed adapter failures, cancellation, and temporary-key disposal.
- `AuthorizationEnvelopePasswordLifecycleTests` covers password policy, first-run refusal to overwrite, typed load and activation failures, current-password proof, quick-unlock preservation, owned result keys, cancellation, and clearing of generated, recovered, and wrapper buffers.
- `PortableAuthorizationServiceTests` covers initialization projection, startup routing, password and platform unlock, password setup, explicit-recovery quick-unlock enrollment, preference rollback, gate selection, password replacement, lock behavior, and temporary context-key clearing.
- `UnavailablePlatformQuickUnlockTests` verifies the Avalonia fallback reports unsupported availability, never claims the Windows provider identity, and returns typed unavailable failures for registration, unlock, and removal.
- `AvaloniaCompositionRootTests` verifies that the portable settings, authorization, enrollment, account-management, platform-path, and file-security services resolve from the desktop host graph.
- `AvaloniaStartupCoordinatorTests` covers settings-before-authorization sequencing, password setup/unlock projection, fail-closed preference loading, and sanitized unexpected failures.
- `MainWindowViewModelTests` covers explicit setup/unlock status and retryable startup failures without exposing underlying exception text.
- `PasswordUnlockViewModelTests` uses synthetic credentials to cover input clearing before asynchronous verification, successful unlock signaling, rate-limit projection, generic invalid-credential handling, and exception-text suppression.
- The focused quick-unlock/security-contract test selection passes 39 tests.
- The focused enrollment test selection passes 14 tests.
- The focused portable-settings test selection passes 6 tests.
- The focused portable-preferences store selection passes 7 tests.
- The focused encrypted-vault and backup persistence selection passes 20 tests.
- The focused authorization-envelope session test selection passes 35 tests.
- The focused authorization-envelope password-lifecycle selection passes 12 tests.
- The focused portable-authorization facade selection passes 18 tests.
- The focused WPF authorization and settings-orchestration selection passes 54 tests.
- The focused portable-authorization and password-setup presentation selection passes 28 tests.
- The focused infrastructure and WPF composition selection passes 3 tests and verifies that the legacy DPAPI settings DAL is absent.
- The focused security-context and active key-consumer selection passes 72 tests.
- The full Debug solution test run passes 718 tests.
- The Release solution build succeeds with zero warnings and errors, and the filtered PR-like Release test run passes 662 tests.
- A real Windows Hello/TPM registration and unlock smoke test remains required on supported hardware before release; automated tests use the existing `IHelloGate` OS boundary.
