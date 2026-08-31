# Avalonia notification policy

Avalonia action feedback uses two shared building blocks:

- `NotificationState` owns text, severity, visibility, replacement, cancellation, and transient lifetime.
- `NotificationBanner` renders that state consistently and supplies severity styling and accessibility live-region behavior.

Notifications remain owned by the narrowest relevant context. Account actions display over the account list; import/export feedback stays in Import/Export; settings persistence stays with the settings section; log-folder feedback stays in Info. Only application-wide lifecycle events use the shell-level banner.

Use persistent notifications for failures, warnings, and states that require user awareness. Use transient notifications for successful or cancelled actions that need no follow-up. Do not bind one notification state into unrelated tabs or workflows.

Inline validation and workflow status are not general notifications. Keep password, account-field, scanner, and generated-code validation beside the affected control or workflow so the user can identify what requires attention.
