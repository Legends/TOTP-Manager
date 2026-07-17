# Clipboard security

## Scope

M1.3 separates clipboard policy from WPF access. `ClipboardBackgroundService` owns copy and timed-clear policy through the portable `IClipboardService`; `WpfClipboard` is the Windows presentation adapter behind `IPlatformClipboard`.

## Security behavior

- Clipboard operations return explicit result codes for unavailable, write-failed, and clear-failed states.
- Auto-clear is offered only when the adapter advertises both text-write and conditional-clear capabilities.
- A successful write returns an opaque change token. The policy retains only that token and the deadline, never a second copy of the OTP text.
- At timeout, the adapter clears only when the platform clipboard change token still matches the original write. If the user or another application replaced the clipboard, clearing is skipped.
- Transient clear failures retain the schedule and are retried on the next polling interval.
- A later unscheduled copy cancels the previous clear schedule.
- Logs record operation type, timing, and error code only. Clipboard contents and change tokens are not logged.
- UI success state is shown only after a successful copy; failures use localized, recoverable messages.

## Platform capability policy

Future macOS and Linux adapters must provide a reliable native change token or equivalent conditional-clear primitive before advertising `ConditionalClear`. If safe replacement detection is unavailable, auto-clear must report `ClipboardUnavailable`; it must not clear clipboard contents unconditionally.

Platform adapters are boundary components and must translate native clipboard/dispatcher failures into result errors without including clipboard content in exception messages or logs.

## Threat impact and limitations

- This reduces the lifetime of OTP material in application-managed memory and avoids deleting unrelated clipboard content.
- The operating-system clipboard is shared state. Other applications running as the same user may read an OTP while it is present.
- Timed clearing is best effort and cannot revoke content already observed or synchronized by clipboard-history/cloud features.
- The Windows sequence number detects replacement but is not a confidentiality control.

## Compatibility

- Default clear duration remains 30 seconds at the service boundary.
- The WPF account workflow continues to use the configured duration, with its existing 15-second fallback.
- Disabling automatic clear still performs a normal clipboard write and cancels any older pending clear.
