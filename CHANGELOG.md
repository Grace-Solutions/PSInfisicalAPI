# Changelog

All notable changes to this project will be documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) loosely, but version numbers use the build timestamp format `yyyy.MM.dd.HHmm`.

## Unreleased

## 2026.06.04.0123

- Build produced from commit 2cbd5c2008f5.

## Unreleased (carried forward)

- **M10 polish Ã¢â‚¬â€ formatting, type metadata, and PKI route aliases**:
  - Added default table views and `DefaultDisplayPropertySet` entries for `InfisicalCertificateAuthority`, `InfisicalCertificate`, and `InfisicalCertificateBundle` in the module `Format.ps1xml` / `Types.ps1xml`.
  - Realigned PKI endpoint registry to current Infisical paths: `ListInternalCertificateAuthorities` and `RetrieveInternalCertificateAuthority` now use `/api/v1/cert-manager/ca/internal[/{caId}]` as primary, with legacy `/api/v1/pki/ca/internal[/{caId}]` retained as a fallback alias. `GetCertificateBundle` and `RetrieveCertificate` similarly carry `cert-manager` fallback aliases.
  - `InfisicalApiInvoker.InvokeWithCandidateFallback` walks the candidate list and falls back on `404`/`405`, used by `InfisicalPkiClient` so older self-hosted Infisical instances are tolerated transparently.

## 2026.06.04.0114

- Build produced from commit 2cbd5c2008f5.

## Unreleased (carried forward)

- **M10 Ã¢â‚¬â€ PKI Internal CAs, Certificates & Windows Store integration**:
  - **`Get-InfisicalCertificateAuthority`** lists internal certificate authorities for the current project, or returns a single CA with `-CaId`.
  - **`Search-InfisicalCertificate`** wraps `POST /api/v1/projects/{projectId}/certificates/search` with rich filters (`-CommonName`, `-FriendlyName`, `-Search`, `-Status`, `-CaId`, `-ProfileId`, `-ApplicationId`, `-EnrollmentType`, `-KeyAlgorithm`, `-SignatureAlgorithm`, `-Source`, `-NotAfterFrom/To`, `-NotBeforeFrom/To`, `-SortBy/-SortOrder`, `-Limit/-Offset`). Auto-paginates unless `-NoAutoPage` is set.
  - **`ConvertTo-InfisicalCertificate`** accepts an `InfisicalCertificate`, `InfisicalCertificateBundle`, or `-SerialNumber`, fetches the bundle endpoint when needed, and emits a `System.Security.Cryptography.X509Certificates.X509Certificate2` with the private key attached. `-NoPrivateKey` skips key parsing; `-IncludeChain` additionally emits intermediates; `-KeyStorageFlags` controls import behavior.
  - **`Install-InfisicalCertificate`** / **`Uninstall-InfisicalCertificate`** perform idempotent installs/removes against a Windows `X509Store` (`-StoreName`, `-StoreLocation`, defaults `My`/`CurrentUser`), matching by thumbprint. Install is a no-op when the thumbprint is already present unless `-Force` is supplied (which replaces the existing entry). Both honor `ShouldProcess` and accept pipeline input.
  - **`Export-InfisicalCertificate`** writes PEM, PFX, or CER to disk via `-Format`, with `-Password` (SecureString) for PFX, `-IncludeChain` for full-chain PEM, `-NoPrivateKey` to omit the key, and `-Force` to overwrite.
  - **BouncyCastle dependency**: Added `BouncyCastle.Cryptography` to bridge PEM/PKCS#8 parsing on .NET Standard 2.0 / Windows PowerShell 5.1 (where `X509Certificate2.CreateFromPem` and `RSA.ImportFromPem` are unavailable). The shared `PemCertificateBuilder` assembles cert + chain + key into an in-memory PKCS#12 blob and imports it back into `X509Certificate2`. The DLL ships in the published module bin directory.
  - PKI endpoint registry entries for `ListInternalCertificateAuthorities` (`GET /api/v1/pki/ca/internal`), `RetrieveInternalCertificateAuthority` (`GET /api/v1/pki/ca/internal/{caId}`), `SearchCertificates` (`POST /api/v1/projects/{projectId}/certificates/search`), `RetrieveCertificate`, and `GetCertificateBundle` (`GET /api/v1/pki/certificates/{serialNumber}/bundle`).

## 2026.06.04.0020

- Build produced from commit 211fbcf34dbb.

## Unreleased (carried forward)

## 2026.06.04.0005

- Build produced from commit e0a6ef02df3e.

## Unreleased (carried forward)

- **Bulk v4 batch routes**: Endpoint registry now registers `POST|PATCH|DELETE /api/v4/secrets/batch` as the preferred candidates for `BulkCreateSecret`/`BulkUpdateSecret`/`BulkDeleteSecret`; the existing v3 raw routes (`/api/v3/secrets/batch/raw`) remain as automatic fallback. Batch request DTOs serialize both `projectId` (required by v4) and `workspaceId` (accepted by v3) when populated.
- **Strongly-typed bulk input**: `-Secrets` on `New-InfisicalSecret` and `Update-InfisicalSecret` is now `IDictionary<string, string>[]` instead of `Hashtable[]`. `InfisicalBulkSecretConverter` accepts `IEnumerable<IDictionary<string, string>>` and parses `TagIds` from a comma-separated string. Nested `Metadata`/`SecretMetadata` dictionaries are no longer accepted in the bulk hashtable surface (set `SecretMetadata` programmatically on `InfisicalBulkCreateSecretItem`/`InfisicalBulkUpdateSecretItem` if needed).

## 2026.06.03.2207

- Build produced from commit 09c3d5c68bbc.
- **M9 ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â Bulk, Duplicate & Inheritance**:
  - **Bulk parameter sets** added to `New-InfisicalSecret`, `Update-InfisicalSecret`, and `Remove-InfisicalSecret` accepting `-Secrets Hashtable[]`; client methods `CreateBatch`/`UpdateBatch`/`DeleteBatch` wrap `POST|PATCH|DELETE /api/v3/secrets/batch/raw`.
  - **`Copy-InfisicalSecret`** cmdlet added, wrapping `POST /api/v4/secrets/duplicate` with source/destination environment + path parameters and per-attribute copy toggles.
  - **Connection inheritance** centralized in `InfisicalCmdletBase` (`ResolveProjectId`/`ResolveEnvironment`/`ResolveSecretPath`/`ResolveApiVersion`/`ResolveOrganizationId`). Explicit parameters always win; missing values fall back to the active connection and emit a `-Verbose` line.
  - Project/Environment/Folder/Tag and all secret cmdlets refactored to use the inheritance helpers; existing explicit-parameter behavior is preserved.
  - `InfisicalBulkSecretConverter` accepts flexible key aliases (`SecretName`/`Name`/`Key`, `SecretValue`/`Value`, `SecretComment`/`Comment`, `Metadata`/`SecretMetadata`).
  - Test count: 161 (up from 139). Added coverage for bulk DTO shapes, the converter, the duplicate request DTO, registry entries for the four new endpoints, and the resolution helpers.

## Unreleased (carried forward)

## 2026.06.03.2206

- Build produced from commit 09c3d5c68bbc.

## Unreleased (carried forward)

## 2026.06.03.2136

- Build produced from commit d9822aab7a4a.
- **Resource CRUD expansion**: Added full Get/New/Update/Remove cmdlet families for Projects, Environments, Folders, and Tags (20 new cmdlets):
  - Projects: `Get-InfisicalProjects`, `Get-InfisicalProject`, `New-InfisicalProject`, `Update-InfisicalProject`, `Remove-InfisicalProject`.
  - Environments: `Get-InfisicalEnvironments`, `Get-InfisicalEnvironment`, `New-InfisicalEnvironment`, `Update-InfisicalEnvironment`, `Remove-InfisicalEnvironment`.
  - Folders: `Get-InfisicalFolders`, `Get-InfisicalFolder`, `New-InfisicalFolder`, `Update-InfisicalFolder`, `Remove-InfisicalFolder`.
  - Tags: `Get-InfisicalTags`, `Get-InfisicalTag`, `New-InfisicalTag`, `Update-InfisicalTag`, `Remove-InfisicalTag`.
- **Secret mutation cmdlets**: Added `New-InfisicalSecret`, `Update-InfisicalSecret`, and `Remove-InfisicalSecret`; extended `InfisicalSecretsClient` with corresponding create/update/delete operations.
- **Additional auth providers**: `Connect-Infisical` now supports JWT (`-Jwt -IdentityId`), OIDC (`-Jwt -IdentityId`), LDAP (`-Username -Password`), Azure (`-Jwt -IdentityId`), and GCP IAM (`-Jwt -IdentityId`) via dedicated parameter sets. Common identity-login flow is centralized in `IdentityLoginExecutor`.
- Endpoint registry expanded with login routes (`/api/v1/auth/{jwt|oidc|ldap|azure|gcp}-auth/login`) and CRUD routes for projects (v2), environments, folders, tags, and secret mutations.
- Test suite expanded to 139 passing tests, including mapper round-trips for projects/environments/folders/tags, secret mutation DTO shapes, and request-body validation for each new auth provider.

## 2026.06.03.0131

- Build produced from commit 7be0b7b42008.
- **Behavior change**: `Get-InfisicalSecrets` and `Get-InfisicalSecret` now default `-ViewSecretValue` to `$true`. Real secret values are returned by default. To request the redacted/hidden response, pass `-ViewSecretValue:$false`.
- `InfisicalSecretMapper` now treats the server-side `<hidden-by-infisical>` placeholder as a hidden marker rather than a value: when `secretValueHidden=true` (or the placeholder string is detected) `SecretValue` is set to `null` instead of stuffing the literal into a `SecureString`. This prevents downstream consumers (auth, exports, dictionary conversion) from silently using `<hidden-by-infisical>` as if it were a real secret.

## Unreleased (carried forward)

## 2026.06.03.0113

- Build produced from commit 09c577ebd0fd.
- Added `InfisicalSecret.GetPlainTextValue()` for direct plain-text access to secret material from PowerShell without needing `Marshal.SecureStringToBSTR`.
- Added `-AsPlainText` switch to `ConvertTo-InfisicalSecretDictionary`; when present the cmdlet emits `Dictionary<string, string>` instead of the default `Dictionary<string, SecureString>`.

## Unreleased (carried forward)

## 2026.06.03.0057

- Build produced from commit 7e5209190ac2.

## Unreleased (carried forward)

## 2026.06.03.0056

- Build produced from commit 7e5209190ac2.

## Unreleased (carried forward)

## 2026.06.03.0055

- Build produced from commit 7e5209190ac2.

## Unreleased (carried forward)

## 2026.06.03.0047

- Build produced from commit 7e5209190ac2.

## Unreleased (carried forward)

## 2026.06.03.0046

- Build produced from commit 7e5209190ac2.

## Unreleased (carried forward)

## 2026.06.03.0032

- Build produced from commit c86676010532.

## Unreleased (carried forward)

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward)

## 2026.06.02.1737

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward) (carried forward)

## 2026.06.02.1724

- Build produced from commit 5801b4774af5.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward)

## 2026.06.02.1737

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward) (carried forward) (carried forward)

## 2026.06.02.1648

- Build produced from commit 430e3a00c921.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward)

## 2026.06.02.1737

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward) (carried forward)

## 2026.06.02.1724

- Build produced from commit 5801b4774af5.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward)

## 2026.06.02.1737

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward) (carried forward) (carried forward) (carried forward)

## 2026.06.02.1638

- Build produced from commit 3c47d6ff30ec.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward)

## 2026.06.02.1737

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward) (carried forward)

## 2026.06.02.1724

- Build produced from commit 5801b4774af5.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward)

## 2026.06.02.1737

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward) (carried forward) (carried forward)

## 2026.06.02.1648

- Build produced from commit 430e3a00c921.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward)

## 2026.06.02.1737

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward) (carried forward)

## 2026.06.02.1724

- Build produced from commit 5801b4774af5.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward)

## 2026.06.02.1737

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward) (carried forward) (carried forward) (carried forward) (carried forward)

## 2026.06.02.1611

- Build produced from commit 3c47d6ff30ec.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward)

## 2026.06.02.1737

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward) (carried forward)

## 2026.06.02.1724

- Build produced from commit 5801b4774af5.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward)

## 2026.06.02.1737

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward) (carried forward) (carried forward)

## 2026.06.02.1648

- Build produced from commit 430e3a00c921.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward)

## 2026.06.02.1737

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward) (carried forward)

## 2026.06.02.1724

- Build produced from commit 5801b4774af5.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward)

## 2026.06.02.1737

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward) (carried forward) (carried forward) (carried forward)

## 2026.06.02.1638

- Build produced from commit 3c47d6ff30ec.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward)

## 2026.06.02.1737

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward) (carried forward)

## 2026.06.02.1724

- Build produced from commit 5801b4774af5.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward)

## 2026.06.02.1737

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward) (carried forward) (carried forward)

## 2026.06.02.1648

- Build produced from commit 430e3a00c921.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward)

## 2026.06.02.1737

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward) (carried forward)

## 2026.06.02.1724

- Build produced from commit 5801b4774af5.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward)

## 2026.06.02.1737

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward)

## 2026.06.02.1902

- Build produced from commit fa65c18bc171.

## Unreleased

## 2026.06.02.1907

- Build produced from commit fa65c18bc171.

## Unreleased (carried forward) (carried forward) (carried forward) (carried forward) (carried forward) (carried forward) (carried forward)

### Added

- Initial repository skeleton, C# `netstandard2.0` project, and PowerShell module layout.
- Centralized logging (`InfisicalLogger`), error types/handler, sanitizer, path utility, and `SecureString` utility.
- Endpoint registry covering `UniversalAuthLogin`, `ListSecrets`, and `RetrieveSecret`, and a `System.Uri`-based URI builder.
- Synchronous HTTP client, JSON/YAML/XML/ENV serializers, and DTO/mapper for secrets.
- Connection model, process-level session manager, Universal Auth and Token Auth providers.
- Cmdlets: `Connect-Infisical`, `Disconnect-Infisical`, `Get-InfisicalSecrets`, `Get-InfisicalSecret`, `ConvertTo-InfisicalSecretDictionary`, `Export-InfisicalSecrets`.
- Build script (`build.ps1`) generating manifest, copying binaries, creating release folders, and supporting unit/integration tests.
- xUnit test project with unit tests and opt-in integration tests.
