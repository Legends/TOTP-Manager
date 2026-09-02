# UI interaction boundaries

Portable workflows describe user intent without depending on Avalonia types:

- `NotificationSeverity` is the portable severity vocabulary shared by banners and dialog requests.
- Presentation view models own `NotificationState`; they do not call a global notification service.
- Desktop adapters implement `IAvaloniaDialogService`, `IAvaloniaFilePicker`, and the owned QR/scanner dialog contracts.
- Password prompts, QR previews, and scanner workflows do not expose native window types outside the desktop project.

The Avalonia desktop project owns dialogs, window ownership, native file pickers, bitmap conversion, and UI-thread dispatch. QR data crosses portable boundaries only in owned encoded buffers; each temporary buffer and decoded bitmap is cleared or disposed when replaced, hidden, closed, locked, or disposed.

## Notification policy

`NotificationState` is the presentation state for recoverable information, success, warning, and error messages. `NotificationBanner` is their common visual and accessibility surface.

Messages remain in the smallest useful context:

- action or validation feedback stays inside its owning section or dialog;
- page-level outcomes stay on the owning page;
- application-wide failures use the shell notification surface.

A notification state is not shared between unrelated settings tabs. Navigation or disposal clears transient context so stale errors cannot appear in another section. View models select localized, presentation-safe text and never expose exception messages or secret-bearing values.
