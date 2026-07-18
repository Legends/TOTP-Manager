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

Theme policy is `RequestedThemeVariant="Default"`, so Avalonia follows the operating-system light/dark preference. Theme-dependent resources are always consumed through `DynamicResource`, allowing live variant changes. A high-contrast dictionary is defined, but automatic platform detection/activation and real-target contrast testing remain open; its presence alone is not acceptance evidence.

The initial shell consumes the tokens and uses a responsive maximum width rather than a fixed width. The existing M3 target record defines 100%, 150%, and 200% scaling tests.

`BusyOverlay` is a reusable shared control with a templated content surface, indeterminate progress, and a polite accessible status. While busy, its content presenter is disabled and visually subdued, preventing pointer or keyboard activation of the underlying shell. The startup shell now consumes this control instead of relying only on command-state conventions.

`ValidationMessage` centralizes information, warning, and error semantics with theme-aware colors, assertive live-region behavior, wrapping, and automatic visibility for non-empty messages. The password gate consumes the error variant; later screens should reuse the control instead of defining local error colors or separate visibility bindings.

`RevealableSecretInput` keeps masking as the fail-closed default and reveals only while its dedicated control is held by pointer or keyboard. Release, capture loss, focus loss, template removal, and clearing the bound value all restore masking. Feature markup cannot bind a persistent revealed state through the control API. The control does not retain a second secret value; it decorates the existing two-way text binding and owns only transient disclosure state.

`QrPreview` is reserved for generated account QR images. It stays out of the visual and accessibility trees when no image is bound, supplies a meaningful image description, and presents an assertive privacy warning by default because the rendered QR contains the OTP seed. The view model continues to own and dispose the bitmap lifetime; the control holds only the displayed image reference and does not copy or encode secret material. Live camera frames intentionally remain a separate presentation because they have different privacy and lifecycle semantics.

`AccountRow` owns the reusable two-column issuer/account layout and derives one meaningful accessibility label from that secret-free metadata. Native list items retain selection, focus, and keyboard behavior; the row does not introduce commands, selection state, identifiers, seeds, or OTP values.

`NotificationBanner` presents explicit information, success, warning, and error states without parsing message text. Nonfatal status changes use polite live-region announcements; errors use assertive announcements. Empty notifications leave the visual and accessibility trees. The shell now projects startup, retry, unlock, lock, and shutdown state through this contract.

`ConfirmationDialogWindow` consumes the shared notification and button styles, supplies default/cancel keyboard behavior, cannot create an unowned top-level window through the dialog service, and keeps decision policy in a testable view model rather than code-behind.

`PasswordDialogWindow` reuses the fail-closed revealable input, validation presentation, busy overlay, default/cancel behavior, and the same serialized owner path. Its view model removes the password from the bound field before validation, converts validator exceptions to caller-supplied safe text, returns no value on cancellation, prevents duplicate completion, and clears its validator and sensitive fields during teardown. A successful managed-string reference is transferred to the caller because the existing workflows require it; callers remain responsible for minimizing its lifetime.

## Security and compatibility impact

- Threat impact: fatal presentation faults now fail closed instead of leaving an authorized shell running in unknown state.
- Threat impact: password disclosure is transient and automatically cancelled at interaction and visual-tree boundaries; it does not change the existing unavoidable managed-string lifetime in the preview unlock view model.
- Threat impact: generated account QR codes now carry an unavoidable on-screen privacy warning and disappear automatically when their disposed image reference is cleared.
- Threat impact: modal password entry is cleared before asynchronous validation and during every teardown path; no password or validator exception detail is logged or projected into dialog errors.
- Diagnostic impact: early and DI logging use the existing redaction pipeline; exception messages are deliberately omitted at the boundary.
- Data-flow impact: no vault, envelope, seed, import/export, or backup format changes.
- Compatibility impact: WPF startup and release behavior are untouched. Avalonia remains framework-dependent and non-release during migration.
- Compatibility impact: localization adds satellite resources only; it does not change persisted settings yet, and unsupported cultures fall back to neutral English.
- Test evidence: boundary tests cover safe logging, authorization lock, fatal shutdown, shutdown failure, domain lock failure, and unobserved-task policy. Main-shell tests cover idempotent shutdown preparation.
- Navigation evidence: tests cover authorization-gated single-page navigation and preservation of account rows while sensitive generated output is cleared.
