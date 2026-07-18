# M4 Avalonia shell and design foundation

## Scope

This slice establishes lifecycle, error-boundary, and visual conventions for the Avalonia client without making it the default or release client. WPF remains unchanged. Physical M3 camera and interactive target records remain postponed and must not be inferred from this M4 work.

## Shell lifecycle

The desktop entry point now starts the existing redacting Serilog pipeline before single-instance coordination or Avalonia initialization and flushes it on every exit path. The DI composition root bridges `ILogger<T>` to that pipeline; registering logging abstractions without a provider is not sufficient.

`AvaloniaExceptionHooks` owns the UI-dispatcher, application-domain, and unobserved-task subscriptions for exactly the lifetime of the application service provider. `AvaloniaExceptionBoundary` contains the testable policy:

- UI-thread faults log only the exception type, lock authorization, and request orderly shutdown with exit code 1.
- Application-domain faults log only type/termination state and make a best-effort authorization lock while the runtime terminates.
- Unobserved task faults are recorded by type and marked observed, but do not terminate or lock an otherwise healthy session.
- Reporting, locking, or shutdown failures never replace the original fatal path.
- Exception messages are not passed to `ILogger`, because messages can contain paths, payloads, passwords, or other user-controlled data.

The main window initializes its view model only on the first open. Window closing synchronously prepares the shell for shutdown: authorization is locked, generated output/account presentation is cleared, camera capture is cancelled and cleared, authorized surfaces are hidden, and service-provider disposal then releases owned resources. Preparation and disposal are idempotent.

After authorization, the shell exposes exactly one of three explicit pages: Accounts, Tools, or Settings. Locking hides the entire authorized shell and returns to the password gate. Leaving Accounts clears generated OTP/QR output while preserving the loaded secret-free rows; leaving Tools cancels and clears camera capture. Settings reload when entered. Navigation commands cannot execute while locked, and the current-page command is disabled to provide a consistent keyboard state.

`AvaloniaWindowCoordinator` is the single owner registry for the main window and active modal dialog. `AvaloniaDialogService` serializes confirmations, always calls Avalonia's asynchronous `ShowDialog` with the registered main-window owner, clears dialog data context after completion, and routes secondary-instance activation to the active modal instead of surfacing an unusable owner behind it. Missing or competing owners fail explicitly. The existing synchronous Core/WPF confirmation contract is not implemented by blocking the Avalonia UI thread; feature workflows will consume the async boundary during their migration.

No splash window is currently justified. The existing shell reports startup state and recoverable failure without adding another lifetime or focus owner. Revisit only if measured interactive startup makes the password gate materially late.

## Localization

Avalonia static shell, dialog, control-label, and automation text is backed by embedded neutral-English and German `.resx` catalogs. At startup, `AvaloniaLocalizationService` maps the operating-system UI culture to the supported language set with deterministic English fallback, then populates the application's existing resource dictionary. Views consume those values through `DynamicResource`, so the Settings language selector updates visible labels and accessibility names in place without recreating windows. The session selection is intentionally not persisted until the cross-client settings schema is reviewed in M5; startup therefore follows the OS again on the next launch. Dynamic workflow messages remain feature-owned and will move into the same catalog as each M5 workflow is migrated.

Malformed or unsupported culture names never select an arbitrary resource or throw during startup. The catalog test requires every declared key to resolve in both initial languages. Translation values contain presentation text only and never receive passwords, seeds, account metadata, or formatted exception details.

## Design tokens

`TOTP.UI.Avalonia.Shared/Styles/SharedStyles.axaml` is the single application-level foundation for:

- semantic light, dark, and high-contrast color roles;
- typography sizes and heading classes;
- a 4-pixel-based spacing scale;
- small, medium, and large radii;
- card elevation;
- fast and standard motion-duration values;
- page, card, and control padding;
- minimum interactive-control height;
- primary, danger, card, heading, secondary-text, and semantic validation styles;
- keyboard-visible focus borders for buttons, text input, numeric input, and account lists.

Theme policy defaults to `RequestedThemeVariant="Default"`, so Avalonia follows the operating-system light/dark preference. Theme-dependent resources are always consumed through `DynamicResource`, allowing live variant changes. A dedicated inherited high-contrast variant supplies its own semantic resources; automated activation is implemented, while real-target contrast testing remains open.

`AvaloniaThemeService` now observes the platform color-values contract for the application lifetime. `NoPreference` keeps Avalonia's system-following default; `High` selects the custom `HighContrast` variant, whose palette uses black surfaces, white text/borders, yellow accents, cyan focus, explicit warning/error/success colors, and no card shadow. Color changes are applied on the UI dispatcher and the subscription is removed on exit. This completes behavior definition and automated policy coverage, but real-target contrast, focus visibility, forced-colors integration, and assistive-technology acceptance remain postponed target evidence.

The initial shell consumes the tokens and uses a responsive maximum width rather than a fixed width. The existing M3 target record defines 100%, 150%, and 200% scaling tests.

The Avalonia asset baseline links the existing application icon for native window identity and supplies a shared `SymbolIcon` control for Add, Camera, Conceal, Copy, Lock, Reveal, Search, and Settings. Symbols are code-native vector geometry, inherit semantic foreground brushes, and therefore scale cleanly and remain visible under theme/high-contrast changes without an SVG renderer dependency. Only icons with a current or near-term shared-control consumer are ported; feature-specific imagery remains with its M5 feature slice.

Shared styles now own normal, primary, secondary, danger, disabled, and focus-visible button states; text, numeric, and combo input surfaces; semantic validation/notification states; and modal window, content, and action layout. Confirmation and password windows consume the same `Window.dialog`, `StackPanel.dialog-content`, and `StackPanel.dialog-actions` contracts instead of repeating chrome properties. New M5 screens should extend these semantic classes only when a genuinely new interaction role appears.

`BusyOverlay` is a reusable shared control with a templated content surface, indeterminate progress, and a polite accessible status. While busy, its content presenter is disabled and visually subdued, preventing pointer or keyboard activation of the underlying shell. The startup shell now consumes this control instead of relying only on command-state conventions.

`ValidationMessage` centralizes information, warning, and error semantics with theme-aware colors, assertive live-region behavior, wrapping, and automatic visibility for non-empty messages. The password gate consumes the error variant; later screens should reuse the control instead of defining local error colors or separate visibility bindings.

`RevealableSecretInput` keeps masking as the fail-closed default and reveals only while its dedicated control is held by pointer or keyboard. Release, capture loss, focus loss, template removal, and clearing the bound value all restore masking. Feature markup cannot bind a persistent revealed state through the control API. The control does not retain a second secret value; it decorates the existing two-way text binding and owns only transient disclosure state.

`QrPreview` is reserved for generated account QR images. It stays out of the visual and accessibility trees when no image is bound, supplies a meaningful image description, and presents an assertive privacy warning by default because the rendered QR contains the OTP seed. The view model continues to own and dispose the bitmap lifetime; the control holds only the displayed image reference and does not copy or encode secret material. Live camera frames intentionally remain a separate presentation because they have different privacy and lifecycle semantics.

`AccountRow` owns the reusable two-column issuer/account layout and derives one meaningful accessibility label from that secret-free metadata. Native list items retain selection, focus, and keyboard behavior; the row does not introduce commands, selection state, identifiers, seeds, or OTP values.

`NotificationBanner` presents explicit information, success, warning, and error states without parsing message text. Nonfatal status changes use polite live-region announcements; errors use assertive announcements. Empty notifications leave the visual and accessibility trees. The shell now projects startup, retry, unlock, lock, and shutdown state through this contract.

`ConfirmationDialogWindow` consumes the shared notification and button styles, supplies default/cancel keyboard behavior, cannot create an unowned top-level window through the dialog service, and keeps decision policy in a testable view model rather than code-behind.

`PasswordDialogWindow` reuses the fail-closed revealable input, validation presentation, busy overlay, default/cancel behavior, and the same serialized owner path. Its view model removes the password from the bound field before validation, converts validator exceptions to caller-supplied safe text, returns no value on cancellation, prevents duplicate completion, and clears its validator and sensitive fields during teardown. A successful managed-string reference is transferred to the caller because the existing workflows require it; callers remain responsible for minimizing its lifetime.

## M5.1 first-run password setup

`ReadyForPasswordSetup` now projects an explicit setup surface instead of a dead-end startup status. `PasswordSetupViewModel` performs only fast required/minimum-length/confirmation checks, clears both bound fields before crossing the authorization boundary, and delegates envelope creation to the existing reviewed `IAuthorizationService.ConfigurePasswordAsync` workflow. Success enters the same authorized Accounts shell used by password unlock. An existing-vault conflict remains fail-closed and explains that data was not replaced and recovery/migration is required.

- Threat impact: no new cryptography or KDF policy is introduced; presentation cannot bypass the existing password lifecycle and vault activation verification.
- Data-flow impact: two temporary managed strings exist for password and confirmation because Avalonia binding and the current authorization contract require them; both bound properties and method locals are cleared at the earliest practical points.
- Compatibility/migration impact: the existing v2 envelope and settings formats are unchanged. `ExistingVaultConflict` explicitly prevents destructive first-run replacement.
- Test evidence: setup tests cover success, pre-validation without storage access, existing-vault recovery messaging, exception redaction, and input clearing; shell tests cover first-run surface projection.

## M5.1 startup quick unlock and password recovery

The Avalonia Windows composition root now selects the existing Windows Hello/TPM adapter and supplies the currently active owned Avalonia window handle to the OS verification prompt. Non-Windows builds continue to register the explicit unavailable adapter; no placeholder provider or plaintext fallback is introduced.

Startup attempts quick unlock only when the verified authorization state records it as the preferred method. The authorized shell is exposed only when the service reports success and its shared state is actually unlocked. Cancellation, unavailable hardware, policy restrictions, retry exhaustion, missing platform keys, inconsistent success state, and other failures all remain on the password gate with explicit recovery messaging. This preserves the master password as the universal recovery path.

- Threat impact: the UI cannot infer authorization from an OS prompt result alone; it requires the authorization service to activate the security context and project unlocked state.
- Data-flow impact: no password, vault key, TPM key reference, or wrapper data is added to presentation state. The native window handle is used only to own the Windows verification prompt.
- Compatibility impact: authorization envelope and preference formats are unchanged. WPF keeps its existing provider. macOS and Linux remain password-only pending reviewed M6 adapters.
- Test evidence: startup tests cover password-only bypass, verified quick-unlock success, every recoverable failure class, and a fail-closed inconsistent-success result. Shell tests verify automatic authorized entry and password-fallback projection. Interactive Windows Hello acceptance remains target evidence and is not claimed by these automated tests.

## M5.1 quick-unlock settings

The authorized Settings surface checks platform availability through `IAuthorizationService`, reports password-only fallback explicitly, and permits enrollment only through the existing reviewed authorization workflow. If a valid platform wrapper already exists, selecting it changes only the preferred startup gate. Otherwise, the owned password dialog requires the current master password and `ConfigureHelloAsync` verifies recovery access before any platform registration is committed.

Changing the startup preference back to password requires master-password reauthorization. It deliberately retains the platform wrapper, matching the existing service contract, so the user may select quick unlock again without creating abandoned TPM keys. The UI labels this as a startup preference rather than claiming that platform enrollment was deleted.

- Threat impact: quick unlock cannot become the only recovery mechanism; enrollment is unavailable without a valid recovery password, and OS/provider failures preserve password access.
- Data-flow impact: the recovery password crosses only the owned dialog and authorization contract, and its presentation references are cleared immediately after the call. No password or platform result detail is logged.
- Compatibility impact: this is an adapter over the existing v2 envelope and preference state machine. It introduces no schema, KDF, wrapper, or migration change.
- Test evidence: tests cover platform availability, enrollment requiring the recovery-password prompt, cancellation without enrollment, and password reauthorization before changing the startup preference. Physical Windows Hello enrollment and cancellation remain target evidence.

## M5.1 password rotation and reauthorization

The security settings surface delegates master-password rotation to `IAuthorizationService.ChangePasswordAsync`; it does not reproduce KDF, envelope replacement, vault verification, or rollback policy in the view model. New-password inputs are checked for required value, minimum length, and exact confirmation, then removed from their bound fields before the owned current-password authorization dialog opens. Inputs are also cleared whenever Settings is left, the vault is locked, or shutdown begins.

Successful rotation preserves the existing quick-unlock wrapper through the reviewed password lifecycle, while the new password remains the universal recovery path. Invalid current credentials and storage/activation failures return sanitized messages and never claim that a change occurred.

- Threat impact: password rotation requires current-password authorization and cannot be triggered from a locked surface. Presentation validation is advisory; the authorization and lifecycle services remain authoritative.
- Data-flow impact: current, new, and confirmation values necessarily exist briefly as managed strings under current contracts. Bound values are cleared before authorization, locals are released at the earliest practical boundary, and none are logged.
- Compatibility/migration impact: the v2 password wrapper is atomically replaced by the existing lifecycle; quick-unlock metadata and rollback behavior are preserved. No schema changes are introduced.
- Test evidence: tests cover input clearing before authorization, successful delegation, mismatch rejection without dialog/storage calls, lock-state shell hiding, and password reauthorization for startup-gate changes.

## Security and compatibility impact

- Threat impact: fatal presentation faults now fail closed instead of leaving an authorized shell running in unknown state.
- Threat impact: password disclosure is transient and automatically cancelled at interaction and visual-tree boundaries; it does not change the existing unavoidable managed-string lifetime in the preview unlock view model.
- Threat impact: generated account QR codes now carry an unavoidable on-screen privacy warning and disappear automatically when their disposed image reference is cleared.
- Threat impact: modal password entry is cleared before asynchronous validation and during every teardown path; no password or validator exception detail is logged or projected into dialog errors.
- Diagnostic impact: early and DI logging use the existing redaction pipeline; exception messages are deliberately omitted at the boundary.
- Data-flow impact: no vault, envelope, seed, import/export, or backup format changes.
- Compatibility impact: WPF startup and release behavior are untouched. Avalonia remains framework-dependent and non-release during migration.
- Compatibility impact: localization adds satellite resources only; it does not change persisted settings yet, and unsupported cultures fall back to neutral English.
- Compatibility impact: high contrast follows the platform preference dynamically and otherwise leaves the existing system light/dark policy unchanged.
- Test evidence: boundary tests cover safe logging, authorization lock, fatal shutdown, shutdown failure, domain lock failure, and unobserved-task policy. Main-shell tests cover idempotent shutdown preparation.
- Navigation evidence: tests cover authorization-gated single-page navigation and preservation of account rows while sensitive generated output is cleared.
