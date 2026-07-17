# Synthetic legacy settings fixtures

These files represent the decrypted JSON payloads written by historical settings models. They contain deterministic synthetic data only.

The fixture password is `Synthetic-Only-Password!42`. It protects the synthetic DEK whose Base64 value is `oaKjpKWmp6ipqqusra6vsLGys7S1tre4ubq7vL2+v8A=`. The PBKDF2-era fixtures use the same password with 200,000 SHA-256 iterations. The envelope-era fixtures use Argon2id with three passes, 65,536 KiB, and parallelism one, followed by AES-256-GCM.

DPAPI ciphertext is deliberately not checked in because it is bound to a Windows user context. Integration tests protect each plaintext fixture with current-user DPAPI at runtime before exercising `AppSettingsDAL`.

The manifest records the source commit and current-reader behavior. `LosesTopLevelAuthorization` and `PasswordHashUnsupported` are compatibility evidence, not acceptable migration outcomes. A future migration reader must use explicit historical models and verify recovery before retiring the legacy file.
