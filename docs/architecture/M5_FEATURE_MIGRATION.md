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
