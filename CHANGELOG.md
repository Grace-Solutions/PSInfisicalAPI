# Changelog

All notable changes to this project will be documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) loosely, but version numbers use the build timestamp format `yyyy.MM.dd.HHmm`.

## Unreleased

- Infisical API error responses are now parsed to surface the server-side `message`, `error`, and `reqId` fields. The 4xx/5xx exception message includes the human-readable explanation (e.g. "The project is of type secret-manager") instead of an opaque `Infisical API returned 400 (Bad Request)`. The `InfisicalApiException` gains `ApiErrorMessage` and `ApiRequestId` properties; `InfisicalErrorDetails` carries the same fields so PowerShell error records and logger output expose them.
- `Get-InfisicalCertificateProfile` added with `List` (default) and `ById` parameter sets. List binds to `GET /api/v1/cert-manager/certificate-profiles` (optional `-Limit`, `-Offset`, `-IncludeConfigs`); ById binds to `GET /api/v1/cert-manager/certificate-profiles/{certificateProfileId}`. New `InfisicalCertificateProfile` model surfaces ca/policy ids, slug, enrollment type, per-profile defaults (ttl, key/extended key usages), and the embedded CA/policy/apiConfig summaries.
- `Get-InfisicalCertificatePolicy` added with `List` (default) and `ById` parameter sets. List binds to `GET /api/v1/cert-manager/certificate-policies` (optional `-Limit`, `-Offset`); ById binds to `GET /api/v1/cert-manager/certificate-policies/{certificatePolicyId}`. New `InfisicalCertificatePolicy` model surfaces subject, SANs, key usages, extended key usages, algorithms, and validity. Polymorphic string-or-array fields (`allowed`, `required`, `keyAlgorithm`) are normalized to arrays; `sans` is normalized whether the API returns an object or an array.
- `Get-InfisicalCertificateAuthority` gains a `-Kind` parameter on the List parameter set with values `Internal` (default, preserves prior behavior against `/api/v1/cert-manager/ca/internal`), `Any` (binds to the generic `/api/v1/cert-manager/ca` endpoint which returns both internal and ACME CAs), and `Acme` (uses the generic endpoint and client-side filters to ACME issuers only). ById retrieval is unchanged and still resolves against the internal CA endpoint.
- `Request-InfisicalCertificate` gains a `ByProfile` parameter set bound by the new `-CertificateProfileId` parameter (alias `ProfileId`). The cmdlet generates a local keypair and CSR as usual, then POSTs to `/api/v1/cert-manager/certificates` with the profile id, the CSR, and a subject/attribute envelope (commonName, organization, organizationalUnit, country, state, locality, ttl, notBefore, notAfter, keyUsages, extendedKeyUsages). The wrapped response (`{certificate:{certificate,certificateChain,issuingCaCertificate,serialNumber,certificateId,privateKey}, certificateRequestId, status, message}`) is unwrapped into the existing `InfisicalSignedCertificate` shape so the install / reuse / chain-completion paths continue to work unchanged. Issuance that returns without a certificate (e.g. status `pending_approval`) raises a configuration exception that surfaces the reported status and message.

## 2026.06.04.1920

- Build produced from commit 0f8f44afdb38.

## Unreleased (carried forward)

- `build.ps1` gains a `-CommitArtifacts` switch that, after a successful build, stages and commits only the build outputs (`Module/PSInfisicalAPI/bin/**`, `Module/PSInfisicalAPI/PSInfisicalAPI.psd1`, and the auto-inserted `CHANGELOG.md` build stamp) with a message that references the source commit whose hash is now embedded in `BuildCommitHash`. The switch is mutually exclusive with the older broader `-CommitOnSuccess` (which still uses `git add -A`). README extended with a "Committing source and build artifacts in lockstep" section describing the recommended two-commit workflow.

## 2026.06.04.1917

- Build produced from commit a34db831d8bf.

## Unreleased (carried forward)

## 2026.06.04.1915

- Build produced from commit 2489b7adca98.

## Unreleased (carried forward)

## 2026.06.04.1911

- Build produced from commit 51bf819c37e5.

## Unreleased (carried forward)

## 2026.06.04.1906

- Build produced from commit 51bf819c37e5.

## Unreleased (carried forward)

- **BREAKING**: Removed the plural-noun discovery cmdlets `Get-InfisicalProjects`, `Get-InfisicalEnvironments`, `Get-InfisicalFolders`, `Get-InfisicalTags`, `Get-InfisicalSecrets`, and `Get-InfisicalCertificates`. Their behavior is now folded into the corresponding singular cmdlets via a `List` (default) / single-record parameter set pair, matching the existing `Get-InfisicalCertificateAuthority` precedent. Callers should drop the trailing `s`; invocation without the identity parameter (`-ProjectId`, `-EnvironmentSlugOrId`, `-FolderNameOrId`, `-TagSlugOrId`, `-SecretName`, `-SerialNumber`) now returns the list, and supplying the identity parameter returns the single record. No back-compat aliases were added.
- Added `Get-InfisicalPkiSubscriber` with `List` (default) and `ByName` parameter sets, backed by new `InfisicalPkiClient.ListPkiSubscribers` and `GetPkiSubscriber` methods, an `InfisicalPkiSubscriber` model, and corresponding DTOs/mapper. Use the emitted `Name` (slug) on `Request-InfisicalCertificate -PkiSubscriberSlug`.
- **Bug fix**: `Request-InfisicalCertificate -PkiSubscriberSlug ...` was returning 404 because the registry's `SignCertificateBySubscriber` endpoint pointed at `/api/v1/pki/pki-subscribers/{subscriberName}/sign-certificate` and `/api/v1/cert-manager/pki-subscribers/...`. Per Infisical's `v1/index.ts`, the subscriber router is mounted at `/pki/subscribers`, so the single correct path is `/api/v1/pki/subscribers/{subscriberName}/sign-certificate`. The redundant `cert-manager` template was removed; the PKI endpoint registry tests were updated to match.
- Updated MAML help in `Module/PSInfisicalAPI/en-US/PSInfisicalAPI.dll-Help.xml`: the six consolidated cmdlets and the new `Get-InfisicalPkiSubscriber` each ship three examples — two straight-line invocations (one per parameter set) plus one `OrderedDictionary` splat example. All in-text references to the removed plural cmdlets across other cmdlets' examples were updated to the singular form.
- `build.ps1`: `CmdletsToExport` and the `Test-ModuleImports` expected cmdlet list were updated to drop the six plural cmdlets and add `Get-InfisicalPkiSubscriber` (total: 34 exported cmdlets).

## 2026.06.04.1825

- Build produced from commit 19615363e356.

## Unreleased (carried forward)

## 2026.06.04.1820

- Build produced from commit 19615363e356.

## Unreleased (carried forward)

- `Install-InfisicalCertificate` now routes chain certificates by self-signed status instead of dumping every chain entry into the Intermediate Certification Authorities store. Self-signed roots are installed into `StoreName.Root` (Trusted Root Certification Authorities) and non-self-signed intermediates are installed into `StoreName.CertificateAuthority` (Intermediate Certification Authorities). The leaf continues to use the user-specified `-StoreName`/`-StoreLocation` (default `My`/`CurrentUser`). `Request-InfisicalCertificate` already routed chain certs correctly; the same routing helper is now shared by both cmdlets.
- `InfisicalCertificateRequestHelpers` exposes a new public `GetChainCertificateTargetStore(X509Certificate2)` classifier and a new `InstallChain(IEnumerable<X509Certificate2>, StoreLocation, bool, IInfisicalLogger, string)` overload. The existing `InstallChain(InfisicalSignedCertificate, ...)` overload now delegates to the new collection-based overload, so PKI chain-installation routing is centralized in one place.

## 2026.06.04.1810

- Build produced from commit 19615363e356.

## Unreleased (carried forward)

- Authored MAML help (`Module/PSInfisicalAPI/en-US/PSInfisicalAPI.dll-Help.xml`) covering all 39 exported cmdlets. Every entry includes a synopsis, description, notes section, and two examples: a one-liner and an `OrderedDictionary` splat (with `OrdinalIgnoreCase`) that includes preceding `Get-` resolver commands wherever IDs or slugs are required.
- `build.ps1` now stages the cmdlet help XML next to the deployed binary. After the publish step, every culture directory under `Module/PSInfisicalAPI/` (matching `xx` or `xx-XX`) that contains `PSInfisicalAPI.dll-Help.xml` is mirrored into `bin/<culture>/`. The script hard-fails if `bin/en-US/PSInfisicalAPI.dll-Help.xml` is missing or contains zero `<command:command>` entries.
- `Test-ModuleImports` in `build.ps1` now dynamically enumerates exported cmdlets via `Get-Command -Module PSInfisicalAPI -CommandType Cmdlet`, cross-checks the result against an expected list of 39 cmdlet names (including the previously-missing `Copy-InfisicalSecret`), and for each cmdlet asserts that `Get-Help -Full` returns a non-empty synopsis (rejecting PowerShell's auto-generated cmdlet-name fallback), a non-empty description, and that `Get-Help -Examples` returns at least one example node whose `<dev:code>` block is non-empty.

## 2026.06.04.1808

- Build produced from commit 19615363e356.

## Unreleased (carried forward)

## 2026.06.04.1658

- Build produced from commit 19615363e356.

## Unreleased (carried forward)

- `Request-InfisicalCertificate` reuse path now falls back to the Infisical certificate-bundle endpoint when the local trust stores do not contain the issuing intermediates or root. The cmdlet builds the local chain first; if the result has no intermediates and no root, it fetches `GetCertificateBundle(serialNumber)` and rebuilds the result with the bundle's chain PEM merged in. A new `-LocalChainOnly` switch opts out of the bundle fetch for strict offline behavior. Bundle-fetch failures are logged at verbose level and the cmdlet returns the local-only result.
- `InfisicalCertificateRequestHelpers.BuildResultFromExistingLocal` adds a second overload that accepts an `InfisicalCertificateBundle`; when supplied, chain certs from the bundle are deduplicated by thumbprint and merged with the locally-resolved chain before classification.

## 2026.06.04.1652

- Build produced from commit 19615363e356.

## Unreleased (carried forward)

## 2026.06.04.1651

- Build produced from commit 19615363e356.

## Unreleased (carried forward)

## 2026.06.04.1634

- Build produced from commit 19615363e356.

## Unreleased (carried forward)

## 2026.06.04.1631

- Build produced from commit 19615363e356.

## Unreleased (carried forward)

## 2026.06.04.1622

- Build produced from commit 19615363e356.

## Unreleased (carried forward)

- **PKI contract fixes and cmdlet expansion**:
  - `InfisicalPkiClient` no longer auto-injects `connection.ProjectId` into PKI CA list/retrieve calls; only the caller's explicit `-ProjectId` is forwarded so that cert-manager primary routes (which do not accept the query parameter) succeed.
  - List/single CA and single certificate response parsing now tolerate raw arrays, wrapper objects (`{certificate: {...}}`, `{certificates: [...]}`), and nested `configuration` blocks. `InfisicalCaMapper` reads CA detail fields from `configuration` first, falling back to top-level.
  - `RetrieveCertificate(connection, identifier)` added on `InfisicalPkiClient`.
- **New cmdlets**:
  - **`Get-InfisicalCertificate`** — single-record retrieval by `-SerialNumber`/`-Id` (mandatory positional).
  - **`Get-InfisicalCertificates`** — listing with light filtering (`-CommonName`, `-FriendlyName`, `-Status`, `-CaId`, `-Limit`, `-Offset`, `-NoAutoPage`). Auto-paginates by default.
  - **`Request-InfisicalCertificate`** — generates a keypair locally (private key never leaves the device), submits a PKCS#10 CSR to either `pki-subscribers/{name}/sign-certificate` (`-PkiSubscriberSlug`) or `ca/{caId}/sign-certificate` (`-CertificateAuthorityId`), and returns a single `InfisicalCertificateResult` object with the leaf and chain pre-classified. The result exposes `Leaf : X509Certificate2`, `Intermediates : X509Certificate2[]`, `Root : X509Certificate2` (nullable), `Chain : X509Certificate2[]` (ordered leaf → intermediates → root, deduplicated by thumbprint), plus pass-through `SerialNumber`, `CertificatePem`, `CertificateChainPem`, and `PrivateKeyPem`. Supports `-Subject` (`IDictionary` with `CN`/`C`/`ST`/`L`/`O`/`OU`/`E` keys) merged with individual `-CommonName`/`-Country`/etc. parameters (individual params win), `-DnsName`/`-IpAddress` SANs (auto-populated from local FQDN when omitted). Idempotency: scans the local `X509Store` for an existing certificate matching `CN` and an Infisical-known serial number; returns the existing certificate wrapped in an `InfisicalCertificateResult` whose `Intermediates`/`Root`/`Chain` are populated by walking the local trust stores via `X509Chain` (no network calls, revocation checks disabled), and whose `CertificatePem`/`CertificateChainPem` are reconstructed from the resolved certs. Reuse is short-circuited unless `-Force` or `-AllowRenewal` (with optional `-RenewalThresholdDays`, default 30) requests a new one. Installation: `-Install` adds the leaf to `-StoreName`/`-StoreLocation` (default `My`/`CurrentUser`); `-InstallChain` additionally places intermediates into `CertificateAuthority` and self-signed roots into `Root` for the same `-StoreLocation`. `-KeyStorageFlags` is passed through to `X509Certificate2` import.
  - **Multi-algorithm CSR support** on `Request-InfisicalCertificate` via split parameters: `-KeyAlgorithm` (`Rsa`/`Ecdsa`/`Ed25519`, default `Rsa`), `-KeySize` (`2048`/`3072`/`4096`, default `2048`, applies to RSA only), `-Curve` (`P256`/`P384`, default `P256`, applies to ECDSA only). Signature algorithms are picked automatically: SHA256WITHRSA for RSA, SHA256WITHECDSA / SHA384WITHECDSA for ECDSA P-256/P-384, and Ed25519 (pure-EdDSA) for Ed25519. The underlying `InfisicalCsrBuilder.Build(subject, dns, ip, options)` API was updated to take an `InfisicalCsrOptions` object in place of the prior `keySize` int.
  - **Sign-certificate endpoint registrations**: `SignCertificateBySubscriber` and `SignCertificateByCa` registered with both `/api/v1/pki/...` and `/api/v1/cert-manager/...` candidate paths and marked `ContainsSecretMaterialInResponse = true`.

## 2026.06.04.1554

- Build produced from commit 19615363e356.

## Unreleased (carried forward)

## 2026.06.04.1512

- Build produced from commit 19615363e356.

## Unreleased (carried forward)

## 2026.06.04.1508

- Build produced from commit 19615363e356.

## Unreleased (carried forward)

- **CI â€” Gitea artifact upload fix**: Replaced `actions/upload-artifact@v4` and `actions/download-artifact@v4` with the Gitea-compatible forks `christopherhx/gitea-upload-artifact@v4` and `christopherhx/gitea-download-artifact@v4` in `.gitea/workflows/publish-psgallery.yml`. The upstream v4 actions abort on Gitea because Gitea is detected as GHES, which the upstream v4 actions do not support (see [go-gitea/gitea#28853](https://github.com/go-gitea/gitea/issues/28853)).

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
