# WinOTP Authenticator maintenance review - June 2026

This review is based on the open GitHub issues in `VladimirAkopyan/Authenticator`, the current UWP codebase, and the Microsoft Store association metadata in `Authenticator/Package.StoreAssociation.xml`.

## Feedback themes from open issues

| Theme | Representative issues | Notes |
| --- | --- | --- |
| Backup, export, restore, and migration | #8, #22, #24 | Users want to move accounts to a new PC and recover from device loss without relying on OneDrive sync. |
| Cloud sync reliability | #1, #3, #6, #24 | The OneDrive SDK dependency is old and user reports indicate setup/sync failures. |
| OTP compatibility | #5, #9 | Users reported valid use cases for non-6-digit TOTP codes and time-related code mismatches. |
| Keyboard and automation accessibility | #12 | Users expect Enter, Space, Ctrl+C, and Ctrl+Insert to copy the focused code. |
| List management | #14, #16, #17 | Users ask for rename, search, and persistent alphabetical sorting. |
| UI density and layout | #7, #10, #11 | Users report oversized list items, layout direction issues, and unclear add-account affordances. |
| Packaging | #13, #15, #21 | Users ask for sideloading/installers; Windows 7 is out of scope for UWP, but `.appinstaller` packaging is feasible. |
| Maintenance status | #20 | Users are asking whether the app is still maintained. A public roadmap/release note would help. |

## Changes included in this PR

This PR intentionally keeps the first change set small and low-risk:

1. Adds a persisted `Digits` value to `Domain.Account` with backwards-compatible defaulting to 6 digits.
2. Updates `Domain.OTP` to generate 1-9 digit codes, defaulting to 6.
3. Parses the `digits` parameter from `otpauth://totp/...` URIs.
4. Preserves scanned and drag-and-drop QR code digit counts when adding accounts.
5. Displays variable-length codes without assuming exactly six digits.
6. Makes account blocks keyboard-focusable and supports Enter, Space, Ctrl+C, and Ctrl+Insert for copy.
7. Adds URI parsing tests for valid and invalid `digits` parameters.

This directly starts addressing #5 and #12, and lays groundwork for import/export because account metadata now carries digit count explicitly.

## Codebase observations

### UWP and dependency age

The solution is still a UWP app targeting Windows 10 SDK `10.0.18362.0` with a minimum target of `10.0.17763.0`. Several dependencies are old, including `Microsoft.OneDriveSDK` 1.2.0, `Microsoft.UI.Xaml` 2.1, and `Newtonsoft.Json` 12.0.2.

### Storage and security

Local account storage uses `Windows.Security.Cryptography.DataProtection.DataProtectionProvider` with the `LOCAL=user` descriptor, which is appropriate for user-scoped local protection. Export/import should therefore be explicit, encrypted, and warning-heavy because exported OTP seeds become portable secrets.

The separate `Encryption.AESEncrypter` implementation uses AES-CBC with a derived IV and no authentication tag. Do not extend that format for new backup/export features. Prefer a versioned export format with authenticated encryption and a strong KDF.

### Reliability

`AccountStorage` contains several broad `catch` blocks, `async void` cleanup, and `throw e;` rethrows. These should be corrected in a reliability-focused PR before major storage or sync changes.

### UX and accessibility

The current account item layout is visually simple but assumes pointer interaction and six-digit codes. This PR adds keyboard copy. Follow-up UX work should add search, sort, density settings, clearer empty-state/add affordances, RTL validation, and accessible labels across the app.

## Modernization backlog

Suggested sequencing:

1. **Patch compatibility and accessibility**
   - Finish manual UI support for digit count.
   - Add keyboard and screen-reader coverage for add/edit flows.
   - Add code generation tests with RFC-compatible vectors.

2. **Backup/export/import**
   - Add encrypted export with clear warnings.
   - Add import preview and duplicate handling.
   - Consider QR and JSON export variants, but never expose seeds without explicit confirmation.

3. **Sync replacement**
   - Reassess the OneDrive SDK integration.
   - Add structured sync errors and telemetry/logging hooks.
   - Decouple sync from local persistence where possible.

4. **List management**
   - Rename service/account labels.
   - Search entries.
   - Persist custom vs alphabetical sorting.

5. **Packaging and release operations**
   - Add CI build validation.
   - Add release notes.
   - Consider `.appinstaller` distribution for users who do not want Store installation.

6. **Platform modernization**
   - Keep this UWP app stable for existing Store users.
   - Evaluate a separate Windows App SDK / WinUI 3 port branch. This is likely a larger rewrite, not a safe single PR.
