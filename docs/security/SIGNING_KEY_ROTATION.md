# Signing credential rotation runbook

## Scope

Windows stable releases use a SignPath Foundation certificate through SignPath's hosted service. The project does not receive or rotate that certificate's private key. CI stores only:

- `SIGNPATH_API_TOKEN`, a signing-request submitter credential; and
- `SIGNPATH_ORGANIZATION_ID`, a non-secret repository variable.

The separate NetSparkle Ed25519 update key and Apple Developer ID/notarization credentials follow their own provider and release procedures.

## SignPath API-token rotation

Rotate the submitter token immediately after suspected exposure, maintainer offboarding, or an unexpected signing request, and at the cadence required by SignPath:

1. Pause stable tag publication without weakening signature checks.
2. Revoke the affected API token in SignPath.
3. Review signing requests, approval records, GitHub workflow runs, repository access, and branch-protection changes for the affected period.
4. Create a replacement token with submitter permission only. It must not approve requests or configure the project.
5. Replace the `SIGNPATH_API_TOKEN` GitHub Actions secret.
6. Perform a controlled signing rehearsal and confirm that manual approval, repository origin, commit, metadata, and Authenticode verification all match the request.
7. Record the rotation and evidence without recording the token.

## Certificate or service incident

If a Foundation certificate, SignPath organization, artifact configuration, or signing policy may be compromised:

1. Stop stable release publication and contact SignPath support and SignPath Foundation immediately.
2. Do not bypass origin verification, manual approval, metadata restrictions, or the Foundation certificate policy.
3. Preserve signing-request and GitHub workflow evidence without downloading or copying private signing material.
4. Follow SignPath Foundation's revocation or replacement instructions.
5. Identify every affected release and publish a security notice when users may be impacted.
6. Re-sign or replace trusted artifacts only through a newly verified policy and certificate path.

## Verification checklist

- The SignPath request identifies the expected repository, tag, commit, artifact configuration, and GitHub-hosted workflow.
- A named approver manually approved the request.
- Every first-party Windows PE has a valid Authenticode signature and expected product metadata.
- Upstream binaries were not signed with the project's Foundation certificate.
- No exportable Windows certificate or password exists in GitHub Actions secrets or repository history.

The local PFX helper scripts under `scripts/release/signing` are not used by the Foundation-backed hosted workflow and must not be wired back into stable publication.
