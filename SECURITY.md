# Security Policy

OTP Harbor handles authentication secrets and encrypted backup material. Please report suspected vulnerabilities privately and avoid exposing sensitive details in public issues, discussions, logs, screenshots, or test data.

## Reporting a vulnerability

Use [GitHub's private vulnerability reporting form](https://github.com/Legends/otp-harbor/security/advisories/new) when it is available for this repository.

If private reporting is unavailable, open a minimal [GitHub issue](https://github.com/Legends/otp-harbor/issues/new) asking the maintainers to establish a private contact channel. Do **not** include vulnerability details, proof-of-concept code, OTP seeds, passwords, keys, encrypted vaults, or release credentials in that issue.

Include the following in the private report when possible:

- affected release or commit
- affected component or workflow
- clear reproduction steps
- expected and observed behavior
- security impact and realistic attack prerequisites
- a minimal proof of concept that contains no real secrets
- suggested remediation, if known

Please allow the maintainers time to reproduce, assess, and remediate the issue before public disclosure.

## Scope

Reports are especially valuable when they concern:

- vault encryption, key derivation, or key lifecycle
- master-password or Windows Hello authorization bypasses
- plaintext secret persistence or logging
- import, export, backup, or recovery weaknesses
- update-feed, appcast-signature, or release-integrity failures
- unintended OTP seed or clipboard disclosure
- insecure filesystem permissions or local privilege-boundary violations

For the documented trust boundaries and security assumptions, review the [threat model](docs/security/THREAT_MODEL.md) and [security verification notes](docs/security/SECURITY_VERIFICATION.md).

## Supported versions

Security fixes are applied to the latest published release line. Older builds may not receive backported fixes; reproduce reports against the latest available release whenever possible.
