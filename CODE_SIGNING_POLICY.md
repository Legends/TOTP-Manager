# Code signing policy

Free code signing provided by [SignPath.io](https://signpath.io/), certificate by [SignPath Foundation](https://signpath.org/).

This policy applies to Windows Authenticode signatures on stable OTP Harbor release artifacts. Release-candidate packages are explicitly identified as unsigned previews and are not distributed through the automatic-update channel.

## Team roles

- Committer and reviewer: [Legends](https://github.com/Legends)
- Approver: [Legends](https://github.com/Legends)

Changes from contributors who do not have commit access require maintainer review before merge. Participation in source control or SignPath administration requires multi-factor authentication. A SignPath signing request requires manual approval; possession of a CI token alone is not sufficient to obtain a signature.

## Build and signing controls

- Release binaries are built from this public repository by the tag-triggered GitHub Actions workflow on GitHub-hosted runners.
- Stable release tags use the documented `v<major>.<minor>.<patch>` format. The release workflow builds and tests the tagged commit before submitting the retained unsigned Windows payload to SignPath.
- SignPath origin verification binds the signing request to the GitHub workflow run and source commit.
- The SignPath artifact configuration signs only first-party `TOTP.*` Windows PE files built from this repository. Bundled upstream dependencies are not signed with the project's certificate.
- Signed first-party binaries must report `OTP Harbor` as their product name and the tag-derived product version. The release workflow verifies metadata and Authenticode status before packaging.
- Signing credentials and certificate private keys are not stored in this repository. The SignPath certificate key remains in SignPath's protected signing service.
- The release is first assembled as a draft and is published only after package, manifest, update-feed, and signature validation succeeds.

Release engineering files are owned through [CODEOWNERS](.github/CODEOWNERS). Changes to the workflow, signing policy, release scripts, or update trust configuration require security-focused review.

## Privacy

See the [privacy policy](PRIVACY.md). OTP Harbor does not transfer vault or usage information to project-operated systems. Stable direct-download packages contact GitHub for update metadata when automatic update checks are enabled; users can disable those checks.

## Verification and incident response

On Windows, users can inspect an extracted executable with:

```powershell
Get-AuthenticodeSignature .\TOTP.UI.Avalonia.Desktop.exe | Format-List Status,StatusMessage,SignerCertificate
```

The status must be `Valid`, and the signer must chain to the certificate used by SignPath Foundation for the project. Release checksums and signed update metadata provide additional transport-integrity evidence; they do not replace Authenticode verification.

Suspected signing-policy violations, compromised release automation, or malicious artifacts must be reported privately as described in [SECURITY.md](SECURITY.md). Maintainers will stop affected releases, cooperate with SignPath Foundation's investigation, and rotate or revoke affected trust material when required.
