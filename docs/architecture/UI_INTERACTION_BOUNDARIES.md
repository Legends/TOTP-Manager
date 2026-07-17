# UI interaction boundaries

M1.5 moves reusable user-interaction contracts into `TOTP.Core`. These contracts describe intent without referencing WPF:

- `IMessageService` and `INotificationUiClient` use portable notification severity and request models.
- `IFileDialogService` receives structured filters rather than a WPF filter string.
- `IPasswordPromptService`, `IQrPreviewService`, and `IQrScannerRunner` expose no window or WPF image types.

The WPF project owns all framework conversion. `NotificationUiClient` selects WPF notification types, styling, icons, sizing, and dispatcher behavior. `FileDialogService` creates the WPF filter string. `PasswordPromptDialogFactory` assigns the main-window owner when it creates a dialog.

QR images cross portable boundaries as encoded byte buffers. The scanner view model clears each camera preview buffer after decoding it into the WPF image used by the view. The main view clears generated QR PNG data when it is replaced, hidden, or disposed. The WPF preview adapter makes and clears a temporary decode copy.

This changes UI ownership and data representation only. Confirmation behavior, notification actions, file formats, password authorization, and QR decoding policy remain unchanged.
