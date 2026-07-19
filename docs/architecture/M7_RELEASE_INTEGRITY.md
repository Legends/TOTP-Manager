# M7 release integrity and update policy

## Scope

M7.1 establishes a shared integrity contract for Avalonia release artifacts before credentialed native publication is enabled. CI now validates native dependency closure, generates target-specific artifact manifests, verifies every manifest hash and size, and fails on known vulnerable NuGet packages. These controls complement platform signing; they do not replace Authenticode, Developer ID, notarization, or Ed25519 update signatures.

## Artifact manifest

`New-ReleaseArtifactManifest.ps1` accepts only the selected Windows x64, macOS ARM64, and Linux x64 artifact names. Every entry records the target, format, byte length, lowercase SHA-256, installation ownership, and update policy. The manifest contains the full source commit and release version and deliberately omits timestamps so identical inputs produce stable content.

`Test-ReleaseArtifactManifest.ps1` rejects duplicate names, traversal, missing files, changed sizes, and changed hashes. The manifest is audit metadata; the release workflow must still sign update payloads and the feed with Ed25519.

## Native dependency validation

The native package validator rejects foreign OpenCV runtimes on every target. Linux additionally runs `ldd` against the app host and OpenCV bridge and rejects unresolved dependencies. macOS runs `otool -L` against both Mach-O files. The packaged native runtime probe still executes separately, so static closure and actual loading are both tested on matching CI runners.

## Update selection policy

The portable updater accepts only a signed feed and signed payload over HTTPS. Portable clients require explicit `sparkle:os` and `sparkle:architecture` fields. The closed channel set is `stable` and `rc`; stable and RC clients cannot consume each other's entries. Unknown client or item channels fail closed.

Distribution ownership is also closed:

- `direct`: the application may check and download a matching signed artifact;
- `package-manager`: application self-update is disabled;
- `store`: application self-update is disabled;
- any unknown value: configuration failure, with no network request.

The Linux DEB is stamped `package-manager`; direct tar and DMG packages are stamped `direct`. RC packages are stamped `rc`. `TOTP_`-prefixed environment variables may override packaged configuration for controlled testing, using the standard double-underscore separator for nested keys.

## Security review

- Threat impact: wrong-platform, wrong-channel, substituted, truncated, and package-manager-bypassing updates are rejected before installation. Known vulnerable NuGet graphs fail CI.
- Data flow: the only new persistent input is non-secret package policy in `appsettings.json`. Update artifacts remain `.part` in a current-user-restricted directory until complete and become `.ready` only after Ed25519 verification.
- Secret impact: no private signing material is introduced. Artifact manifests contain public release metadata and hashes only.
- Compatibility: the direct package default is stable. Linux DEB installs remain package-manager-owned. Existing WPF appcast behavior is unchanged; Avalonia uses `appcast-v2.xml`.
- Recovery: interrupted or invalid downloads are deleted. Invalid distribution policy fails before network access. A missing or invalid manifest fails release validation.

## Automated evidence

- stable/RC channel isolation;
- wrong-OS artifact rejection;
- strict malformed and substituted appcast signature rejection;
- managed-package no-network behavior;
- interrupted and tampered download cleanup;
- artifact mutation detection after manifest generation;
- target-native dependency checks on matching CI runners;
- solution-wide NuGet vulnerability enumeration.

Physical clean-machine installation, signed macOS notarization, and target update handoff remain explicit M7 acceptance gates.
