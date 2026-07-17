# UI scheduling and application lifetime

## Scope

M1.4 moves UI-thread scheduling and application termination contracts into `TOTP.Core`:

- `IUiScheduler` provides framework-neutral access checks, queued posts, and awaited synchronous/asynchronous invocation.
- `IApplicationLifetime` provides graceful shutdown and explicit process-exit operations.

The WPF client implements these contracts with `WpfDispatcherService` and `WpfApplicationLifetime`. Future Avalonia clients must provide their own adapters at their composition roots.

## Scheduling semantics

- `Post` queues work without waiting when a WPF dispatcher is active. It is intended for property-change and UI-refresh notifications.
- `InvokeAsync(Action)` schedules synchronous UI work and completes after the action runs.
- `InvokeAsync(Func<Task>)` schedules asynchronous UI work and unwraps its task so failures and completion propagate to the caller.
- `CheckAccess` reports whether the caller may touch UI-owned state directly.
- In a headless/test context without an active WPF application, the WPF adapter executes inline. Reusable workflows do not inspect `Application.Current` themselves.

## Lifetime semantics

- View models and workflows request shutdown only through `IApplicationLifetime`.
- `WpfApplicationLifetime` marshals graceful shutdown to the WPF dispatcher and ignores duplicate requests after dispatcher shutdown begins.
- `ExitProcess` remains an explicit emergency/final-close operation that flushes Serilog before terminating the process.
- Direct exit calls that occur before dependency injection exists remain confined to the executable startup and unhandled-exception boundaries in `Program`/`BootLoader`.

## Migration and compatibility impact

- Dispatcher priorities used by the WPF adapter remain `DataBind` for posted work and `Background` for awaited work.
- Debounce, QR scanner state delivery, main-view warmup/timer updates, and auto-update UI callbacks retain their prior UI-thread behavior.
- Auto-update shutdown handoff and main-window close now use the lifetime boundary instead of accessing the WPF application singleton.
- No vault, settings, backup, export, or update-state formats change.

## Reliability and security impact

- Central scheduling makes thread-affinity behavior testable and prevents portable workflows from acquiring hidden WPF dependencies.
- Awaited scheduler calls propagate exceptions rather than losing them in fire-and-forget dispatcher work.
- Central lifetime handling reduces inconsistent shutdown paths that could skip cleanup or logging flushes.
- These contracts do not authorize feature code to terminate the process; callers still use shutdown only at existing fatal or explicit-close boundaries.
