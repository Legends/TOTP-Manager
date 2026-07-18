# M3 real-target validation record

Copy this file once per supported target and name the copy `m3-real-<rid>-<short-commit>.md`. Use only the synthetic M3 vault and QR fixtures. Never place a real seed, password, generated OTP, local account name, or secret-bearing screenshot in the record.

## Identity

| Field | Value |
|---|---|
| Result (`PASS` only when every required row passes) | `INCOMPLETE` |
| Commit | |
| RID | |
| Package SHA-256 | |
| Package workflow run | |
| Measurement artifact/report | |
| OS and version | |
| Architecture | |
| Hardware class | |
| Camera model/connection | |
| Display and scaling | |
| Desktop/session type | |
| Screen reader and version | |
| Tester | |
| UTC timestamp | |

## Automated package evidence

- [ ] The commit matches the package, JSON measurement report, and workflow run.
- [ ] The target package probe loaded OpenCV and constructed decoder/capture objects.
- [ ] Every automated budget passed.
- [ ] The package hash above was calculated before interactive testing.
- [ ] No secret-bearing diagnostics were emitted.

## Interactive measurements

Record raw samples or attach a non-secret report; do not record only an average.

| Measurement | Iterations | p50 | p95 | Acceptance | Result |
|---|---:|---:|---:|---:|---|
| Cold launch to password gate | 10 | | | <= 4,000 ms | |
| Warm launch to password gate | 20 | | | <= 2,500 ms | |
| Search input to visible 500-account result | 20 | | | <= 100 ms | |
| Scan action to first camera preview | 10 | | | <= 3,000 ms | |
| Framed synthetic QR to validated metadata | 10 | | | <= 2,000 ms | |

| Steady measurement | Observed | Acceptance | Result |
|---|---:|---:|---|
| Working set after 5 minutes with 500 accounts | | <= 350 MiB | |
| Camera open/cancel/close cycles | | 100/100 and immediately reopenable | |

## Functional and failure-path record

- [ ] Launch reaches the password gate.
- [ ] Synthetic vault unlock renders 500 accounts.
- [ ] Filtering works by issuer and account name.
- [ ] TOTP generation and timed clipboard clearing work for the current display server.
- [ ] Generated QR image can be shown and disposed.
- [ ] Camera starts only after the explicit scan action.
- [ ] A synthetic TOTP QR decodes to issuer/account metadata without displaying its seed.
- [ ] Cancel stops preview and releases the camera.
- [ ] Camera disable/removal produces the expected typed recovery message.
- [ ] Native file picker retains only the selected file name and does not import.
- [ ] A second process activates the first without changing lock state.

## Display and accessibility matrix

| Check | 100% | 150% | 200% | Notes |
|---|---|---|---|---|
| No clipped or unreadable content | | | | |
| Window remains operable at minimum size | | | | |
| Keyboard reaches every M3 action with visible focus | | | | |
| No keyboard focus trap | | | | |
| Screen reader exposes control names and state | | | | |
| Status/errors are announced | | | | |
| Account rows announce issuer and account | | | | |

## Observations and disposition

Record failures with reproducible non-secret steps and an issue/commit reference. A workaround does not convert a failed required row into a pass.

- Failures:
- Resource/handle observations after 100 camera cycles:
- Platform-specific observations:
- Follow-up references:

Final result: `INCOMPLETE`
