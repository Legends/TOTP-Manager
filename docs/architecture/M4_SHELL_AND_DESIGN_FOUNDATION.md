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

No splash window is currently justified. The existing shell reports startup state and recoverable failure without adding another lifetime or focus owner. Revisit only if measured interactive startup makes the password gate materially late.

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
- primary, danger, card, heading, secondary-text, and validation-error styles;
- keyboard-visible focus borders for buttons, text input, numeric input, and account lists.

Theme policy is `RequestedThemeVariant="Default"`, so Avalonia follows the operating-system light/dark preference. Theme-dependent resources are always consumed through `DynamicResource`, allowing live variant changes. A high-contrast dictionary is defined, but automatic platform detection/activation and real-target contrast testing remain open; its presence alone is not acceptance evidence.

The initial shell consumes the tokens and uses a responsive maximum width rather than a fixed width. The existing M3 target record defines 100%, 150%, and 200% scaling tests.

`BusyOverlay` is a reusable shared control with a templated content surface, indeterminate progress, and a polite accessible status. While busy, its content presenter is disabled and visually subdued, preventing pointer or keyboard activation of the underlying shell. The startup shell now consumes this control instead of relying only on command-state conventions.

`ValidationMessage` centralizes information, warning, and error semantics with theme-aware colors, assertive live-region behavior, wrapping, and automatic visibility for non-empty messages. The password gate consumes the error variant; later screens should reuse the control instead of defining local error colors or separate visibility bindings.

## Security and compatibility impact

- Threat impact: fatal presentation faults now fail closed instead of leaving an authorized shell running in unknown state.
- Diagnostic impact: early and DI logging use the existing redaction pipeline; exception messages are deliberately omitted at the boundary.
- Data-flow impact: no vault, envelope, seed, import/export, or backup format changes.
- Compatibility impact: WPF startup and release behavior are untouched. Avalonia remains framework-dependent and non-release during migration.
- Test evidence: boundary tests cover safe logging, authorization lock, fatal shutdown, shutdown failure, domain lock failure, and unobserved-task policy. Main-shell tests cover idempotent shutdown preparation.
- Navigation evidence: tests cover authorization-gated single-page navigation and preservation of account rows while sensitive generated output is cleared.
