# M3 measurement budgets

## Scope

The automated probe is a repeatable technical gate, not a substitute for interactive startup, rendering, accessibility, or camera-device measurements. `Measure-M3Target.ps1` runs from a published Avalonia package and records process launch, native runtime loading, synthetic account filtering, working set, package size, and native dependency footprint. `Test-M3MeasurementBudget.ps1` enforces the preliminary M3 budgets.

## Automated budgets

| Measurement | M3 budget |
|---|---:|
| First packaged technical probe | <= 10,000 ms |
| Subsequent packaged probe p95 | <= 5,000 ms |
| Probe working-set p95 | <= 256 MiB |
| 500-account filter p95 | <= 10 ms |
| 1,000-account filter p95 | <= 20 ms |
| 5,000-account filter p95 | <= 100 ms |
| Framework-dependent technical package | <= 400 MiB |
| Native dependency footprint | <= 250 MiB |

The process probe deliberately includes OpenCV native loading and 500/1,000/5,000-account filter samples. These generous technical ceilings are intended to detect architectural regressions and packaging explosions across heterogeneous hosted runners. M8 must establish tighter release budgets from interactive clean-machine evidence.

The portable dependency boundary currently contains zero WPF types: Core, DAL, Infrastructure, the shared Avalonia presentation project, and the camera module all target platform-neutral `net9.0`, and the Ubuntu/macOS CI jobs compile the Avalonia host against those projects. This is rechecked by every cross-platform build rather than inferred from a Windows build.

## Real-target budgets

Record these from a packaged synthetic-vault run on every supported target:

| Measurement | M3 acceptance |
|---|---:|
| Cold launch to interactive password gate, p95 of 10 | <= 4,000 ms |
| Warm launch to interactive password gate, p95 of 20 | <= 2,500 ms |
| Unlocked 500-account steady working set after 5 minutes | <= 350 MiB |
| Search input to visible results with 500 accounts, p95 | <= 100 ms |
| Explicit scan action to first camera preview, p95 of 10 | <= 3,000 ms |
| Framed synthetic QR to validated result, p95 of 10 | <= 2,000 ms |
| Camera open/cancel/close cycle | 100/100 successful; device immediately reopenable |
| High-DPI | No clipping or unreadable text at 100%, 150%, and 200% |
| Keyboard | All M3 actions reachable with visible focus and no trap |
| Screen reader | Controls expose names, state, errors, and account-row content |

Measurements must identify commit, package RID, OS version, architecture, hardware class, display/session characteristics, iteration count, and p50/p95 where applicable. Never use a real OTP seed in measurement fixtures or reports.

## Interpretation

- A CI pass proves only the automated rows.
- A target checkbox requires the real-target record and interactive budgets.
- Camera timing and disposal require a physical camera; synthetic or hosted-runner substitutes cannot close that gate.
- Package/signing/notarization release budgets remain M7/M8 work even when the unsigned M3 package is technically viable.

Checked-in baselines:

- [`evidence/m3-win-x64-09da29e.json`](evidence/m3-win-x64-09da29e.json) — clean `09da29e` Windows x64 package, 10 iterations, all automated budgets passed.
