# M5 Avalonia feature migration

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
