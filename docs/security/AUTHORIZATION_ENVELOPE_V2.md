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

## Reader rules

A v2 reader must reject the payload before key derivation when:

- required fields are missing, null, duplicated, or represented with the wrong JSON type;
- `format`, envelope `version`, or either algorithm identifier is unsupported;
- Argon2 version is not 19;
- numeric values are outside implementation security bounds;
- Base64 is invalid or decoded lengths are invalid;
- trailing non-whitespace content exists;
- the AES-GCM authentication check fails.

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
- Platform quick-unlock metadata will be added as an optional, explicitly typed section in the next M2 design step.

## Threat review

The clean version discriminator removes the legacy type-confusion risk. Explicit algorithm identifiers and KDF units reduce cross-platform interpretation errors. Persisting parallelism prevents a hidden implementation default from changing derived keys. Fixed associated data domain-separates the password wrapper without a legacy fallback. Remaining risks are denial of service from hostile KDF values, file replacement/rollback, and metadata tampering; bounded validation, current-user file protection, authenticated unwrap, vault verification, and later atomic-write/backup work address those risks.
