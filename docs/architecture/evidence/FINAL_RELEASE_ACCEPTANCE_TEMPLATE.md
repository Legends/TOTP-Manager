# Final cross-platform release acceptance record

Copy this file for the exact release candidate. Use only a synthetic vault, synthetic QR seed, and disposable update/install directories. Never record passwords, seeds, generated codes, local account names, native-store values, or secret-bearing screenshots.

## Candidate identity

| Field | Value |
| --- | --- |
| Overall result (`PASS` only when every required target passes) | `INCOMPLETE` |
| Release tag/version | |
| Commit | |
| Workflow run | |
| Signed aggregate manifest SHA-256 | |
| Appcast signature verified | |
| Tester(s) | |
| UTC test window | |

## Exact artifacts

| RID | Artifact | SHA-256 matches manifest | Platform signature/notarization | Clean install/update result | Detailed record |
| --- | --- | --- | --- | --- | --- |
| `win-x64` | self-contained ZIP | | | | |
| `win-x64` | fast framework-dependent ZIP | | | | |
| `osx-arm64` | signed/notarized/stapled DMG | | | | |
| `linux-x64` | tar.gz | | N/A; Ed25519 release metadata | | |
| `linux-x64` | DEB | | N/A; signed release manifest | | |

## Required linked records

- [ ] Windows checklist in `docs/architecture/M6_PHYSICAL_ACCEPTANCE.md` completed.
- [ ] macOS checklist in `docs/architecture/M6_PHYSICAL_ACCEPTANCE.md` completed.
- [ ] Ubuntu live-stick checklist in `docs/architecture/M6_PHYSICAL_ACCEPTANCE.md` completed for the observed X11/Wayland session.
- [ ] One `M3_REAL_TARGET_RECORD_TEMPLATE.md` copy completed for each supported RID, including raw performance samples and accessibility/display rows.
- [ ] Windows signed update, relaunch, synthetic-vault preservation, and obstructed-copy recovery completed.
- [ ] macOS Gatekeeper, notarization ticket, LocalAuthentication/Keychain reset fallback, camera permission, and Finder install completed.
- [ ] Linux desktop icon/entry, Secret Service diagnostics, session lock, package ownership, install/upgrade/uninstall, and vault preservation completed.
- [ ] Extended timer/idle-lock soak completed without a secret-bearing presentation leak.
- [ ] External security review findings are linked and release blockers resolved or explicitly accepted by the owner.

## Release decision

- Blocking failures:
- Accepted residual risks and owner:
- Follow-up issues:
- Evidence locations:

Final result: `INCOMPLETE`

Approval name/date:
