# Branch Protection Setup

## Goal
Require security and quality checks before merge to `master` and maintained release branches.

## Branching and Merge Policy
- Trunk-based development: `master` is the single integration branch.
- Use short-lived branches only when needed and integrate back quickly.
- Open PR to `master` for review and checks.
- Required checks must pass before merge.
- Direct pushes to protected branches are disallowed by policy.

## Required Checks
- `build-test`
- `Workflow Lint (actionlint)`
- `Dependency Review (PR)`
- `SAST (CodeQL C#)`
- `Dependency Vulnerability Scan (.NET)`
- `Secret Scan (Gitleaks)`

These checks map to:
- `.github/workflows/build-and-test.yml`
- `.github/workflows/security-audit.yml`

## One-Step Setup (PowerShell)
From repository root:

```powershell
.\scripts\security\Set-GitHubTokenForCurrentUser.ps1
.\scripts\security\Set-BranchProtection.ps1 -Owner "Legends" -Repo "otp-harbor" -Branch "master" -RequireLastPushApproval:$false
.\scripts\security\Set-BranchProtection.ps1 -Owner "Legends" -Repo "otp-harbor" -Branch "release/1.x" -RequireLastPushApproval:$false
```

## Solo vs Team Mode
Solo developer (recommended for 1-person repo):
```powershell
.\scripts\security\Set-BranchProtection.ps1 -Owner "Legends" -Repo "otp-harbor" -Branch "master" -RequireLastPushApproval:$false
```

Team mode (enforce another reviewer for the latest push):
```powershell
.\scripts\security\Set-BranchProtection.ps1 -Owner "Legends" -Repo "otp-harbor" -Branch "master" -RequireLastPushApproval:$true
```

## Token Storage (Current Windows Account Only)
Recommended options:

1. DPAPI file (default)
- Command:
```powershell
.\scripts\security\Set-GitHubTokenForCurrentUser.ps1
```
- Storage: `%APPDATA%\TOTP-Manager\github-token.dpapi`

The token secret and DPAPI path retain their legacy names after the OTP Harbor rebrand so existing maintainer credentials continue to resolve. The script's default repository name is `otp-harbor`.
- Protection: encrypted by Windows DPAPI, bound to the current user profile.

2. SecretStore (optional)
- Prerequisite (current user):
```powershell
Install-Module Microsoft.PowerShell.SecretManagement -Scope CurrentUser
Install-Module Microsoft.PowerShell.SecretStore -Scope CurrentUser
Register-SecretVault -Name SecretStore -ModuleName Microsoft.PowerShell.SecretStore -DefaultVault
```
- Store token:
```powershell
.\scripts\security\Set-GitHubTokenForCurrentUser.ps1 -UseSecretStore
```
- Secret name used by script: `GitHub.Token.TOTPManager`

`Set-BranchProtection.ps1` resolves token in this order:
1. `-Token` parameter
2. `GITHUB_TOKEN` environment variable
3. SecretStore secret `GitHub.Token.TOTPManager`
4. DPAPI file `%APPDATA%\TOTP-Manager\github-token.dpapi`

## What the script enforces
- Require pull request before merging
- Require 1 approving review
- Dismiss stale approvals on new commits
- Require approval of last push (configurable via `-RequireLastPushApproval`)
- Require conversation resolution
- Require branch to be up to date before merge
- Enforce protections for admins
- Prevent force-push and branch deletion
- Require linear history

The build/test workflow runs for every pull request targeting a protected branch. Do not add path filters that prevent the required `build-test` check from being created for documentation-only changes.

Note:
- The script auto-detects repository owner type (`Organization` vs personal user).
- Org-only fields (`dismissal_restrictions`, `bypass_pull_request_allowances`) are only sent for organization repositories to avoid GitHub API `422` validation errors.

## Verification
After running, verify in GitHub:
`Settings -> Branches -> Branch protection rules` (or Rulesets if your org uses rulesets).

## Release tag protection

Create an active repository ruleset targeting `refs/tags/v*` that restricts tag updates and deletions. Stable and release-candidate tags are immutable release evidence: correct a failed release with a new version or RC number instead of moving an existing tag.

Verify the active ruleset before a stable release:

```powershell
gh api repos/Legends/otp-harbor/rulesets
```

## Disable Branch Protection (Script)
If you need to temporarily remove branch protection:

```powershell
.\scripts\security\Remove-BranchProtection.ps1 -Owner "Legends" -Repo "otp-harbor" -Branch "master"
```

Use this only for emergency maintenance. Re-enable protections immediately afterward.
