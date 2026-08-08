# M5 Avalonia feature migration

The closure matrix is recorded in `M5_FEATURE_PARITY.md`.

## M5.1 Authorization and recovery

The authorization and recovery implementation and its security review are recorded in `M4_SHELL_AND_DESIGN_FOUNDATION.md` because the first feature slice was built directly on that shell boundary. The persisted v2 contract remains documented in `docs/security/AUTHORIZATION_ENVELOPE_V2.md`.

## M5.2 Account list and CRUD

The Avalonia client uses a native `ListBox` with an explicit `VirtualizingStackPanel`, native selection and keyboard behavior, a reusable secret-free account row, and issuer/account search over the projected rows. Ordering remains owned by `IAccountManager.GetAllOtpEntriesSortedAsync`, keeping persistence and presentation policy out of XAML. Toolbar and native context-menu actions share the same command state.

Account create and edit use an explicit editor rather than data-grid inline mutation. This keeps the Base32 secret in one clearly bounded input, supports reveal-only-while-held behavior, and avoids putting secret-bearing domain objects into the list item model. Editing reloads only long enough to obtain the selected account, then copies its secret into the editor. Saving removes the secret from the bound field before validation or persistence; cancel, page navigation, lock, clear, and disposal discard it.

Issuer is required and secrets must pass the shared Core Base32 validator. Duplicate identity is defined as the same trimmed issuer and account name, case-insensitively, excluding the account being edited. This permits multiple accounts for one issuer when their account names differ while rejecting accidental duplicate identities. Persistence remains delegated to `IAccountManager`; expected and exceptional failures produce generic localized messages without including secret material or underlying error detail.

Deletion resolves the selected secret-free row back to the current domain account only after an owned warning confirmation. Cancellation performs no data access beyond the already displayed row, while confirmation failure or persistence failure leaves the encrypted store unchanged and reports a recoverable status.

- Threat impact: list rows and selection contain only identifier, issuer, and account name; secret material is never added to row, search, context-menu, confirmation, or notification state.
- Data-flow impact: create/edit secrets necessarily exist briefly as managed strings under the current model and binding contracts. Bound secret values are cleared before write calls and at every editor teardown boundary.
- Compatibility impact: the existing encrypted vault format, account model, DAL, backup policy, and WPF behavior are unchanged.
- Test evidence: tests cover 500-row secret-free projection, 10,000-row projection/filter budget, case-insensitive search, create normalization and pre-write clearing, duplicate rejection without write, edit identity preservation and clearing, owned-confirmation deletion, failure sanitization, and navigation/clear teardown.

## M5.3 TOTP and clipboard

Generating a code starts one cancellable countdown owned by the selected account. The view model displays the current period and remaining seconds through a non-live progress bar, refreshes from `IAccountTotpService` at the period boundary, and replaces the visible code only after a successful result for the same still-selected account. Selection change, navigation, lock, clear, disposal, or a refresh failure cancels the lifetime and removes the code. A manual regeneration first cancels the prior lifetime, preventing competing refresh loops.

The generated OTP itself is not a live-region announcement: screen readers receive a polite “code ready/refreshed” status and can explicitly navigate to the code control. Per-second progress changes are not live announcements, avoiding repeated disclosure and notification spam. This policy preserves discoverability without speaking a sensitive code unexpectedly.

Copy delegates to `IAsyncClipboardService` for the exact remaining code lifetime. That service requires both write and conditional-clear capabilities, associates the scheduled clear with a platform receipt, and clears only if clipboard ownership/content is unchanged. User replacement therefore is never overwritten. Unsupported ownership semantics fail closed with a localized message rather than leaving a code copied without a safe clear guarantee.

- Threat impact: OTPs remain short-lived in presentation and clipboard state; lock/navigation removes the visible value, and conditional clear never destroys newer user clipboard content.
- Data-flow impact: the code crosses only the TOTP result, visible code property, and conditional clipboard boundary. It is not logged, placed in notification text, or included in accessibility labels.
- Compatibility impact: TOTP algorithm, period calculation, account/vault formats, and WPF behavior are unchanged.
- Test evidence: tests cover generation by selected identifier, period-boundary refresh, failure sanitization, selected-state clearing, copy duration, conditional-clear capability/replacement behavior, and bitmap/editor teardown sharing the same lock path.

## M5.4 QR workflows

Generated QR images continue to use the secret-bearing preview control and are disposed on selection, navigation, lock, and view-model teardown. Camera capture remains explicitly user initiated, owns one cancellable session, throttles decode work, detects stalled/disconnected devices, clears encoded preview buffers after UI transfer, and maps missing runtime, missing camera, permission denial, and device loss to recoverable states.

Decoded text now crosses directly into `IQrAccountImportService`; the camera view model never stores or displays the URI or secret. The service rejects payloads above 4 KiB, invalid Base32, non-TOTP URIs, and TOTP parameters the current account model cannot persist accurately. Only SHA-1, six digits, and a 30-second period are accepted until algorithm/digit/period fields are added to the encrypted account schema. This prevents a successful-looking import that would later generate incorrect codes.

New identities are added directly. An exact issuer/account/secret match is reported unchanged. A matching issuer/account with a different secret opens one owned three-way decision: update the existing identifier, keep both with a new identifier, or cancel. The decision callback receives issuer/account metadata only; persistence and secret comparison remain in Infrastructure. Successful mutation raises a metadata-free event that reloads the account list.

- Threat impact: QR secrets never enter notification, conflict-dialog, list-row, logging, or accessibility text. Unsupported TOTP semantics fail before vault access.
- Data-flow impact: the decoded URI and parsed secret necessarily exist briefly in scanner/import locals and the domain account passed to `IAccountManager`; no long-lived presentation property is added.
- Compatibility impact: the encrypted account schema remains unchanged, so unsupported algorithm/digit/period combinations are rejected rather than silently downgraded.
- Test evidence: parser/validator tests cover malformed, oversized, and unsupported payloads; import tests cover add, exact duplicate, update, keep-both, cancel, and pre-storage rejection; camera tests cover safe preview transfer, typed platform failures, cancellation, import delegation, and exception-detail suppression. Physical camera permission and target acceptance remain postponed M3 evidence.

## M5.5 Settings

The Avalonia Settings surface now edits the complete allowlisted `AppPreferencesV1` projection: culture, logging threshold, idle timeout, session/minimize locks, conditional clipboard policy and maximum lifetime, QR preview scale, interface-size override, encrypted-export defaults, export-location behavior, and secret visibility. System DPI scaling remains the default; a bounded accessibility multiplier is applied before Avalonia platform initialization and therefore requires an application restart. Save validates codec bounds before persistence and restores the complete prior projection on expected or exceptional failure rather than partially retaining edited runtime values.

Clipboard policy remains fail-closed: disabling conditional clear disables OTP copy in Avalonia instead of copying without cleanup. When enabled, the scheduled lifetime is the smaller of the code's remaining validity and the configured maximum. QR scale updates the secret-bearing preview only after a successful settings save. Logging-level text explicitly notes that the bootstrap logger applies the new threshold after restart.

Language changes still apply live through dynamic resources and are now persisted through the reviewed culture field. Startup applies the loaded culture before authorization projection; malformed/unsupported cultures continue to fall back through the catalog policy. Security settings remain a separate authorization view model so general preference persistence cannot bypass password/quick-unlock workflows.

About/diagnostics exposes the non-secret informational version and opens the adapter-provided log directory through an injected platform launcher. It never displays the path or passes it through a shell command string. Signed update verification remains available from the Tools surface and does not weaken signature policy.

- Threat impact: settings cannot enable unsafe plaintext clipboard copying, bypass authorization, or expose filesystem paths in status text.
- Data-flow impact: only the existing allowlisted non-secret preference fields are persisted; no password, wrapper, seed, OTP, or account metadata enters preferences.
- Compatibility impact: the existing v1 preference schema is fully consumed without a version change. WPF and Avalonia continue to share it.
- Test evidence: tests cover complete preference persistence, all-or-nothing restoration, bounds, immediate language application, persisted culture startup, clipboard disable enforcement, version presence, path-only launcher delegation, and DI composition.

## M5.6 Import, export, and backup

Import and export now cross a path-independent storage boundary. The Avalonia picker retains the native `IStorageFile` capability and exposes read/write streams plus optional local-path metadata; it does not reduce a sandboxed selection to a path or retain an import filename without access to its contents. The Core export contract therefore has stream-first operations, while its original path methods remain compatibility wrappers for WPF and existing callers. The service owns neither caller-provided stream and supports non-seekable provider streams.

All imports are bounded to 5 MiB before parsing or decryption. The read buffer, decrypted bytes, encoded passwords, plaintext export bytes, and generated ciphertext buffers are cleared after use. Boundary logs now contain only an operation stage and exception type, never selected paths, payload text, passwords, seeds, or exception objects. The encrypted `TOTP` envelope layout, Argon2id parameters, AES-256-GCM use, format marker, and legacy-header reader remain unchanged.

The Avalonia export workflow exposes only encrypted portable backup creation. One owned password dialog requires a minimum-length password and matching confirmation, clears both bound inputs before validation, and warns that recovery is impossible without that password. Native save streams support sandboxed macOS providers. When a local path is available, platform access restrictions are applied after the output stream closes; failure is reported explicitly. The configured open-export-location behavior delegates only the containing directory to the platform launcher and never displays the path.

The native import picker discovers encrypted `.totp` backups and compatibility `.json`, `.txt`, and `.csv` files. Encrypted backups are authenticated before leaving the password dialog. Before mutation, every target is checked for a non-empty bounded issuer, bounded account name, valid normalized Base32 secret, a non-empty identifier replacement, and the 10,000-account safety limit. The UI previews the number of conflicts and the selected skip, replace, or keep-both policy. Identity matching uses identifier first and then the trimmed issuer/account pair; replacement preserves the existing identifier, and keep-both assigns a fresh identifier and collision-free imported issuer.

After confirmation, the workflow requires the existing encrypted-vault rotation backup to succeed before the first write. Portable encrypted backups remain discoverable and recoverable through the same native picker. A write failure is counted and reported rather than hidden; successful mutations raise a metadata-free reload event. The existing per-account persistence contract does not provide an atomic bulk transaction, so the pre-import encrypted recovery generation is the recovery boundary for a partially failed bulk import.

- Threat impact: the new workflow does not persist plaintext secrets, does not expose selected paths or parser/crypto details, validates all imported targets before mutation, and refuses to begin mutation without a recovery backup.
- Data-flow impact: export passwords and account secrets necessarily enter short-lived managed strings under the existing model and binding contracts. Bound password fields and mutable byte buffers are cleared at their boundaries; native storage items and streams are disposed by the workflow.
- Compatibility impact: WPF path callers retain their original API and byte-compatible export envelope. Avalonia can import path-generated WPF backups, and path callers can import stream-generated backups. Plaintext formats remain import-compatible but are not offered by the Avalonia export UI.
- Test evidence: service tests cover path/stream interoperability in both directions, all encrypted payload formats, non-seekable streams, size enforcement, case-insensitive portable filename detection, malformed inputs, and caller stream ownership. Unix-targeted tests exercise pathless encrypted streams and filename-only format detection. Presentation tests cover confirmation-required passwords, pre-write backup enforcement, target normalization, replace-ID preservation, file permission hardening, export-folder launching, account reload events, and suppression of boundary exception details.

## M5.7 Notifications and diagnostics

Avalonia uses the shared severity-aware `NotificationBanner` for persistent information, success, warning, and error state. Polite live-region behavior remains the default, while errors are assertive. Owned confirmation, choice, password, and single-action message dialogs share one serialized dialog boundary; recoverable message dialogs expose only a close action and leave the non-modal banner available if the dialog boundary itself fails.

Startup diagnostics are an allowlisted, in-memory projection. The startup coordinator records only the preferences, authorization, optional quick-unlock, and completed stages with rounded elapsed milliseconds and a success flag. It never records passwords, wrappers, paths, account metadata, exception messages, machine identity, or arbitrary caller-provided stage names. Cancellation retains normal cancellation semantics rather than being reported as a product failure.

The support diagnostics service exposes application version, OS family, process architecture, framework description, a boolean indicating whether the configured log directory exists, and the allowlisted startup records. The Tools surface formats those fields as read-only text and explicitly states that account data and filesystem paths are excluded. It does not expose the username, machine name, full OS description, log path, vault path, environment variables, command line, or camera/device details.

Logging continues to use the OS adapter's application paths and the redacting formatter for bootstrap and host sinks. Defense-in-depth redaction now covers quoted JSON-style sensitive properties and entire `otpauth://` URIs in addition to key/value pairs, query parameters, and bearer tokens. Startup, camera, export, and fatal boundaries continue to log exception type and allowlisted stage only instead of exception objects or messages.

- Threat impact: support output is schema-bound and contains no secret-bearing or user-identifying path fields; redaction covers the structured payload shapes most likely to contain OTP seeds if a future caller logs unsafe text.
- Data-flow impact: only stage names from a closed enum, coarse timing, success flags, runtime identifiers, version, and a log-directory availability boolean enter diagnostics state. Diagnostics are memory-only.
- Compatibility impact: vault, authorization, preferences, export, logging path, and release formats are unchanged. Existing platform-specific application path adapters remain authoritative.
- Test evidence: tests cover startup stage recording, last-record snapshot behavior, support-output path exclusion, safe diagnostic failure mapping, success/error banner severity, recoverable single-action dialog projection, JSON/URI/key-value/bearer redaction, and existing Windows/macOS/Linux application path contracts.

## M5.8 Auto-update UI

The Avalonia updater is now a stateful client of `IPortableUpdateService`, not a NetSparkle/WPF UI wrapper. Check, explicit download, cancellation, progress, release notes, verified-package-ready, installer-started, and recoverable failure states are presentation-owned and testable without network or process execution. A check never starts a download, and a download never starts installation. Release notes are rendered as bounded plain text rather than remote HTML.

Infrastructure owns feed and package trust. An enabled feed must use HTTPS and provide an Ed25519 public key, separately fetched appcast signature, and a signed appcast. Portable clients require every selected enclosure to declare an explicit OS and architecture, preventing a generic WPF artifact from being offered to Linux, macOS, or an Avalonia Windows package. The selected enclosure must also carry a correctly shaped `sparkle:edSignature` before download is enabled.

Downloads use response streaming, a 128 MiB hard limit, a generated application-data staging name, current-user directory/file restrictions, cancellation, and best-effort partial cleanup. The complete staged payload is verified with Ed25519 before it is renamed to a ready package. Invalid signatures never produce an installable state. The ready-package contract carries the expected signature and public key so each future platform installer adapter can reverify immediately before process handoff and close the post-download TOCTOU window.

Installer execution is isolated behind `IUpdateInstallerLauncher`. M5 intentionally registers a fail-closed unavailable adapter: the existing production updater is Windows/WPF-specific, and no Avalonia release packages or platform installers have been approved yet. The UI therefore reports a verified ready package and an explicit unsupported-installer warning instead of attempting to run the WPF updater or treating a downloaded archive as installed. Windows, macOS, and Linux installer adapters belong to M6 packaging/platform work.

The current Avalonia composition root supplies no production feed configuration, so update checking reports disabled by default. This prevents the preview client from consuming the existing WPF release channel. A future package may enable the service only with its own target-qualified signed appcast entries and installer adapter.

- Threat impact: neither an unsigned appcast, generic cross-target artifact, unsigned package, oversized response, HTTP feed, partial download, nor unsupported installer can reach installation.
- Data-flow impact: public appcast/package signatures, public key, bounded release notes, target identifiers, progress counts, and the private ready-package path cross the update service boundary. The UI never exposes the URL or local path.
- Compatibility impact: the existing WPF NetSparkle workflow, production key, appcast parser defaults, and updater process are unchanged. The new explicit-target requirement is requested only by the portable client.
- Test evidence: verifier tests preserve the existing signed fixture and reject generic artifacts when explicit targeting is requested. Portable service tests generate Ed25519 fixtures, verify both appcast and package, enforce HTTPS, reject tampered payloads, and assert partial cleanup. View-model tests cover no implicit download, release notes, progress-ready state, adapter failure sanitization, and verification failure.
