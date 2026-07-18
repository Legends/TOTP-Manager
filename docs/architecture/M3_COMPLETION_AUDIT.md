# M3 completion audit

## Status

M3 implementation and automated validation are complete. Formal M3 acceptance remains open because the three supported targets do not yet have completed physical-camera and interactive validation records. This distinction is intentional: hosted native loading proves package feasibility, not permission prompts, capture behavior, rendering, or assistive-technology behavior.

## Requirement-to-evidence matrix

| Requirement | Authoritative evidence | Status |
|---|---|---|
| Avalonia shared and desktop projects, composition root, WPF remains default | `TOTP.UI.Avalonia.Shared`, `TOTP.UI.Avalonia.Desktop`, solution/build workflow, startup composition tests | Complete |
| Startup/error boundary and synthetic password unlock | Avalonia startup and password-unlock view-model tests | Complete |
| 500-account list, filtering, TOTP, clipboard clearing, lock, settings, and QR generation | Avalonia presentation tests and full Windows/Unix test runs | Complete |
| UI-neutral camera boundary | `TOTP.Camera.OpenCv`; Core contracts contain no OpenCV or WPF types | Complete |
| Typed camera failures, cancellation, stalls, disposal, and device loss | OpenCV runner and Avalonia camera view-model regression tests | Complete for deterministic boundaries |
| Seed-safe QR validation and presentation | `IQrPayloadValidator`, `QrPayloadValidator`, scanner view model/tests, security verification notes | Complete |
| Camera permission, preview, decode, cancel/reopen, hot-plug, and 100-cycle reliability | One completed real-target record per supported RID | Missing physical evidence |
| Native file picker and single-instance activation | Presentation/transport tests plus Windows, Ubuntu, and macOS CI | Complete |
| Signed test-appcast update check | Ed25519 verifier tests, NetSparkle interoperability test, tamper/size/XML/OS/architecture tests | Complete |
| Target-shaped Windows x64, Linux x64, and macOS ARM64 packages | Workflow-dispatch run `29646133644` and its retained artifacts | Complete for unsigned technical packages |
| macOS application metadata | Packager-generated `Info.plist`, CI `plutil` validation, camera usage description | Complete |
| Native dependency load on supported targets | Three successful packaged `--m3-native-probe` jobs | Complete |
| Automated startup, working set, filtering, package, and native-footprint budgets | Checked-in `c850bb6` JSON reports and budget validator | Complete |
| Interactive startup, rendered-list latency, and steady memory | Completed real-target measurement tables | Missing interactive evidence |
| High-DPI, keyboard, and screen-reader behavior | Completed display/accessibility matrix for each supported RID | Missing interactive evidence |
| Accessibility implementation path | Scrollable minimum-size layout, automation names, required-field semantics, live status/error regions | Complete as implementation path; target behavior pending |
| WPF leakage into portable/shared boundary | Platform-neutral target frameworks and successful Ubuntu/macOS compilation | Zero detected |
| Full build, tests, security checks, and push verification | Build run `29646239524`; security run `29646239521` | Complete |

## Decision-gate audit

| Decision | Current conclusion |
|---|---|
| Replacement list/control strategy acceptable | Not proven until rendered-list, keyboard, scaling, and screen-reader target rows pass. |
| QR scanning works on all supported desktop systems | Not proven until all three physical-camera records pass. |
| Packaging feasible | Proven for unsigned framework-dependent technical packages on Windows x64, Ubuntu 24.04 x64, and macOS ARM64. |
| No security-storage blocker | The M3 slice uses synthetic data and does not alter vault, envelope, import/export, or backup formats; regression suites are green. |
| Accessibility has a viable implementation path | Proven structurally; real assistive-technology behavior remains a target acceptance gate. |
| Startup and memory acceptable | Automated technical budgets pass; interactive release-facing budgets remain a target acceptance gate. |

## Closure procedure

1. Download the three packages produced by workflow-dispatch run `29646133644` before their retention expires, or dispatch the workflow again from the commit selected for testing.
2. Copy `evidence/M3_REAL_TARGET_RECORD_TEMPLATE.md` once for each supported RID.
3. Record package SHA-256 and all required target identity fields before testing.
4. Run every functional, performance, camera, DPI, keyboard, and screen-reader row using synthetic fixtures only.
5. Resolve every failed required row; a workaround does not count as a pass.
6. Commit the three completed records and update the open M3 checkboxes only when their evidence says `PASS`.
7. Re-run the completion audit and decision gate before beginning broad M4 UI migration.
