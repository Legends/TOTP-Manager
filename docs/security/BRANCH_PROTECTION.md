# Branch Protection Setup

## Goal
Require security and quality checks before merge to `master`.

## Required Checks
- `build-test`
- `sast-codeql`
- `sca-dotnet`
- `secrets-scan`

These checks map to:
- `.github/workflows/build-and-test.yml`
- `.github/workflows/security-audit.yml`

## One-Step Setup (PowerShell)
From repository root:

```powershell
.\scripts\security\Set-GitHubTokenForCurrentUser.ps1
.\scripts\security\Set-BranchProtection.ps1 -Owner "Legends" -Repo "TOTP-Manager" -Branch "master"
```

## Token Storage (Current Windows Account Only)
Recommended options:

1. DPAPI file (default)
- Command:
```powershell
.\scripts\security\Set-GitHubTokenForCurrentUser.ps1
```
- Storage: `%APPDATA%\TOTP-Manager\github-token.dpapi`
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
- Require approval of last push
- Require conversation resolution
- Require branch to be up to date before merge
- Enforce protections for admins
- Prevent force-push and branch deletion
- Require linear history

Note:
- The script auto-detects repository owner type (`Organization` vs personal user).
- Org-only fields (`dismissal_restrictions`, `bypass_pull_request_allowances`) are only sent for organization repositories to avoid GitHub API `422` validation errors.

## Verification
After running, verify in GitHub:
`Settings -> Branches -> Branch protection rules` (or Rulesets if your org uses rulesets).
