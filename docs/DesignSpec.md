# PSInfisicalAPI Full Specification

## 1. Project Summary

`PSInfisicalAPI` is a C# binary PowerShell module for interacting with the Infisical REST API. It is inspired by `PSInfisical`, but it is **not** a PowerShell-only implementation. The module must be built as a compiled C# module targeting `.NET Standard 2.0` so it works in both Windows PowerShell 5.1 and PowerShell 7+.

The goal is to establish a strong, reusable, secure framework first, then initially implement secret retrieval and export workflows.

Public cmdlets:

```powershell
Connect-Infisical
Disconnect-Infisical
Get-InfisicalSecrets
Get-InfisicalSecret
New-InfisicalSecret
Update-InfisicalSecret
Remove-InfisicalSecret
Copy-InfisicalSecret
ConvertTo-InfisicalSecretDictionary
Export-InfisicalSecrets
Get-InfisicalProjects
Get-InfisicalProject
New-InfisicalProject
Update-InfisicalProject
Remove-InfisicalProject
Get-InfisicalEnvironments
Get-InfisicalEnvironment
New-InfisicalEnvironment
Update-InfisicalEnvironment
Remove-InfisicalEnvironment
Get-InfisicalFolders
Get-InfisicalFolder
New-InfisicalFolder
Update-InfisicalFolder
Remove-InfisicalFolder
Get-InfisicalTags
Get-InfisicalTag
New-InfisicalTag
Update-InfisicalTag
Remove-InfisicalTag
Get-InfisicalOrganization
New-InfisicalOrganization
Update-InfisicalOrganization
Remove-InfisicalOrganization
Get-InfisicalSubOrganization
New-InfisicalSubOrganization
Update-InfisicalSubOrganization
Remove-InfisicalSubOrganization
```

Infisical’s public API is REST-based and provides programmatic access for managing secrets and related resources. Current Infisical documentation shows the list-secrets endpoint under `/api/v4/secrets`, the single-secret retrieval endpoint under `/api/v4/secrets/{secretName}`, and Universal Auth login under `/api/v1/auth/universal-auth/login`. The implementation must centralize API endpoint definitions because Infisical uses different API versions across resource families. ([Infisical Blog][1])

---

# 2. Non-Negotiable Requirements

## 2.1 Runtime

The module must target:

```text
.NET Standard 2.0
PowerShellStandard.Library
Windows PowerShell 5.1
PowerShell 7+
```

## 2.2 No Async/Await

The codebase must contain **no** usage of:

```csharp
async
await
```

All HTTP calls, file writes, serialization, logging, and export operations must be synchronous.

## 2.3 Centralized Reusable Logic

No double implementation.

The following must be centralized:

```text
Logging
Error handling
Endpoint definitions
URI construction
Query string construction
HTTP request execution
Authentication
Response parsing
Secret conversion
SecureString handling
Export formatting
Path handling
Version handling
Build/release handling
```

No endpoint URL, API path, query construction logic, or authentication flow should be scattered across cmdlets.

## 2.4 Secret Safety

The module must prioritize limiting secret exposure in memory.

Rules:

```text
Never log secret values.
Never log client secrets.
Never log access tokens.
Never log Authorization headers.
Never log raw request bodies that contain secrets.
Never log raw API response bodies that contain secrets.
Never expose plaintext secret values as public object properties.
Never expose plaintext through ToString().
Never expose raw API response JSON from public cmdlets.
Convert secret values to SecureString as quickly as practical.
Call MakeReadOnly() on SecureString values after population.
Clear temporary API response objects as aggressively as practical.
Clear temporary plaintext variables as aggressively as practical.
```

The module should document the unavoidable .NET limitation: once a secret exists as a managed `string`, that memory cannot be reliably zeroed. Therefore, the design goal is to avoid unnecessary copies, avoid logging, keep plaintext scope short, and move values into read-only `SecureString` objects as quickly as practical.

## 2.5 No Export Warnings

Do **not** emit warning messages for export operations.

The user intentionally requested export support. The module should silently perform the export, while still ensuring exported values are never written to verbose/debug/error logs.

This applies to:

```text
JSON
YAML
ENV
XML
EnvironmentVariables
```

---

# 3. Repository Structure

The repository must follow this structure:

```text
PSInfisicalAPI/
├── Artifacts/
├── Module/
│   └── PSInfisicalAPI/
│       ├── PSInfisicalAPI.psd1
│       ├── PSInfisicalAPI.psm1
│       ├── PSInfisicalAPI.Format.ps1xml
│       ├── PSInfisicalAPI.Types.ps1xml
│       └── bin/
│           ├── PSInfisicalAPI.dll
│           ├── Newtonsoft.Json.dll
│           └── YamlDotNet.dll
├── Releases/
│   └── yyyy.MM.dd.HHmm/
├── docs/
│   ├── about_PSInfisicalAPI.help.txt
│   ├── Connect-Infisical.md
│   ├── Disconnect-Infisical.md
│   ├── Get-InfisicalSecrets.md
│   ├── Get-InfisicalSecret.md
│   ├── ConvertTo-InfisicalSecretDictionary.md
│   └── Export-InfisicalSecrets.md
├── src/
│   ├── PSInfisicalAPI/
│   │   ├── Authentication/
│   │   ├── Cmdlets/
│   │   ├── Common/
│   │   ├── Connections/
│   │   ├── Endpoints/
│   │   ├── Errors/
│   │   ├── Exports/
│   │   ├── Http/
│   │   ├── Logging/
│   │   ├── Models/
│   │   ├── Secrets/
│   │   ├── Security/
│   │   └── Serialization/
│   └── PSInfisicalAPI.Tests/
├── build.ps1
├── CHANGELOG.md
└── README.md
```

Source starts under `/src`.

Namespaces should follow responsibility and folder depth, for example:

```text
PSInfisicalAPI.Authentication
PSInfisicalAPI.Cmdlets
PSInfisicalAPI.Endpoints
PSInfisicalAPI.Security
PSInfisicalAPI.Serialization
```

---

# 4. Module Files

## 4.1 PSD1

The manifest must be generated by the build script.

Example shape:

```powershell
@{
    RootModule = 'PSInfisicalAPI.psm1'
    ModuleVersion = 'yyyy.MM.dd.HHmm'
    GUID = '<stable-guid>'
    Author = 'Grace Solutions'
    CompanyName = ''
    Copyright = ''
    PowerShellVersion = '5.1'
    CompatiblePSEditions = @('Desktop', 'Core')
    FunctionsToExport = @()
    CmdletsToExport = @(
        'Connect-Infisical',
        'Disconnect-Infisical',
        'Get-InfisicalSecrets',
        'Get-InfisicalSecret',
        'New-InfisicalSecret',
        'Update-InfisicalSecret',
        'Remove-InfisicalSecret',
        'Copy-InfisicalSecret',
        'ConvertTo-InfisicalSecretDictionary',
        'Export-InfisicalSecrets',
        'Get-InfisicalProjects',
        'Get-InfisicalProject',
        'New-InfisicalProject',
        'Update-InfisicalProject',
        'Remove-InfisicalProject',
        'Get-InfisicalEnvironments',
        'Get-InfisicalEnvironment',
        'New-InfisicalEnvironment',
        'Update-InfisicalEnvironment',
        'Remove-InfisicalEnvironment',
        'Get-InfisicalFolders',
        'Get-InfisicalFolder',
        'New-InfisicalFolder',
        'Update-InfisicalFolder',
        'Remove-InfisicalFolder',
        'Get-InfisicalTags',
        'Get-InfisicalTag',
        'New-InfisicalTag',
        'Update-InfisicalTag',
        'Remove-InfisicalTag'
    )
    AliasesToExport = @()
    PrivateData = @{
        PSData = @{
            Tags = @('Infisical', 'Secrets', 'API', 'SecureString')
            ProjectUri = ''
            ReleaseNotes = ''
            CommitHash = '<git-commit-hash>'
        }
    }
}
```

## 4.2 PSM1

The `.psm1` must be minimal and only load the binary module plus format/type data.

```powershell
$BinaryPath = [System.IO.FileInfo][System.IO.Path]::Combine($PSScriptRoot, 'bin', 'PSInfisicalAPI.dll')

Import-Module -Name $BinaryPath.FullName

$TypesPath = [System.IO.FileInfo][System.IO.Path]::Combine($PSScriptRoot, 'PSInfisicalAPI.Types.ps1xml')
$FormatPath = [System.IO.FileInfo][System.IO.Path]::Combine($PSScriptRoot, 'PSInfisicalAPI.Format.ps1xml')

if ([System.IO.File]::Exists($TypesPath.FullName)) {
    Update-TypeData -PrependPath $TypesPath.FullName -ErrorAction SilentlyContinue
}

if ([System.IO.File]::Exists($FormatPath.FullName)) {
    Update-FormatData -PrependPath $FormatPath.FullName -ErrorAction SilentlyContinue
}
```

---

# 5. Versioning

Version format:

```text
yyyy.MM.dd.HHmm
```

Example:

```text
2026.06.02.2140
```

The version must be generated once per build and applied consistently to:

```text
PSD1 ModuleVersion
AssemblyVersion
AssemblyFileVersion
AssemblyInformationalVersion
Release folder
CHANGELOG.md
Generated docs if applicable
```

The git commit hash must be embedded separately:

```text
PSD1 PrivateData.PSData.CommitHash
AssemblyMetadata("CommitHash", "<commit-hash>")
AssemblyInformationalVersion = "yyyy.MM.dd.HHmm"
```

---

# 6. Build Script Specification

The build script must be:

```text
Idempotent
Repeatable
Safe to run multiple times
Responsible for versioning
Responsible for manifest generation
Responsible for release folder creation
Responsible for module folder creation
Responsible for copying binaries
Responsible for optional commit-on-success
```

## 6.1 Build Script Parameters

```powershell
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [switch]$Clean,

    [switch]$Restore,

    [switch]$RunTests,

    [switch]$RunIntegrationTests,

    [switch]$CreateRelease,

    [switch]$CommitOnSuccess,

    [switch]$Force
)
```

## 6.2 Required Behavior

```text
Create missing folders.
Clean generated output when -Clean is used.
Never delete source files.
Generate version once.
Read current git commit hash.
Restore packages when requested.
Build the C# project.
Run unit tests when requested.
Run integration tests only when explicitly requested.
Generate PSD1.
Generate PSM1 if missing or when -Force is used.
Copy compiled DLLs to Module/PSInfisicalAPI/bin.
Copy dependency DLLs to Module/PSInfisicalAPI/bin.
Copy type and format files.
Update CHANGELOG.md.
Create Releases/yyyy.MM.dd.HHmm when requested.
Overwrite same-version release only when -Force is used.
Validate module import where possible.
Commit only after successful build when -CommitOnSuccess is specified.
Never commit failed builds.
```

## 6.3 Example Usage

```powershell
.\build.ps1 -Clean -Restore -RunTests -CreateRelease -CommitOnSuccess
```

```powershell
.\build.ps1 -Clean -Restore -RunTests -RunIntegrationTests -CreateRelease
```

## 6.4 Periodic Build and Commit Rule

After each logical milestone:

```text
Build.
Test.
If successful, commit.
If failed, do not commit.
```

Suggested commit messages:

```text
Add module skeleton and build script
Add centralized logging and error handling
Add endpoint registry and URI builder
Add connection manager and auth providers
Add secret retrieval cmdlets
Add secret export providers
Add integration test support
```

---

# 7. Test Instance Configuration

Integration tests must read connection values from any scope environment variables with precendence of Process, User, Machine. First found wins.

Target:

```powershell
[System.EnvironmentVariableTarget]::Machine
```

Required variables:

```text
CLOUDINIT_INFISICAL_APIURL
CLOUDINIT_INFISICAL_ORGANIZATIONID
CLOUDINIT_INFISICAL_PROJECTID
CLOUDINIT_INFISICAL_ENVIRONMENT
CLOUDINIT_INFISICAL_CLIENTID
CLOUDINIT_INFISICAL_CLIENTSECRET
```

## 7.1 Integration Test Rules

```text
Integration tests must not run by default.
Integration tests run only with -RunIntegrationTests.
Missing test environment variables should skip integration tests or fail with a sanitized setup error.
Do not log CLOUDINIT_INFISICAL_CLIENTSECRET.
Do not log access tokens returned by Infisical.
Convert CLOUDINIT_INFISICAL_CLIENTSECRET to SecureString immediately.
Call MakeReadOnly() after SecureString population.
Clear temporary string references as aggressively as practical.
```

Example internal loading pattern:

```csharp
string apiUrl = Environment.GetEnvironmentVariable("CLOUDINIT_INFISICAL_APIURL", EnvironmentVariableTarget.Machine);
string organizationId = Environment.GetEnvironmentVariable("CLOUDINIT_INFISICAL_ORGANIZATIONID", EnvironmentVariableTarget.Machine);
string projectId = Environment.GetEnvironmentVariable("CLOUDINIT_INFISICAL_PROJECTID", EnvironmentVariableTarget.Machine);
string environment = Environment.GetEnvironmentVariable("CLOUDINIT_INFISICAL_ENVIRONMENT", EnvironmentVariableTarget.Machine);
string clientId = Environment.GetEnvironmentVariable("CLOUDINIT_INFISICAL_CLIENTID", EnvironmentVariableTarget.Machine);
string clientSecretPlainText = Environment.GetEnvironmentVariable("CLOUDINIT_INFISICAL_CLIENTSECRET", EnvironmentVariableTarget.Machine);

SecureString clientSecret = SecureStringUtility.ToReadOnlySecureString(clientSecretPlainText);

clientSecretPlainText = null;
```

---

# 8. Logging Specification

Logging must be centralized.

Format:

```text
[UTC Timestamp] - [Level] - [Component] - Message
```

Example:

```text
[2026-06-02T21:44:22.1830000Z] - [Information] - [SecretsClient] - Attempting to retrieve Infisical secrets. Please Wait...
[2026-06-02T21:44:22.9290000Z] - [Information] - [SecretsClient] - Infisical secrets retrieval was successful.
[2026-06-02T21:44:22.9330000Z] - [Error] - [SecretsClient] - Infisical secrets retrieval failed.
```

## 8.1 Required Log Levels

```text
Information
Verbose
Debug
Warning
Error
```

Warning exists as a log level but must not be used for intentional export operations.

## 8.2 Operation Logging

For meaningful operations, log:

```text
Attempting to ...
... was successful.
... failed.
```

Use `Please Wait...` where appropriate.

Examples:

```text
Attempting to authenticate to Infisical. Please Wait...
Infisical authentication was successful.
Infisical authentication failed.

Attempting to retrieve Infisical secrets. Please Wait...
Infisical secrets retrieval was successful.
Infisical secrets retrieval failed.

Attempting to export Infisical secrets to JSON. Please Wait...
Infisical secrets export to JSON was successful.
Infisical secrets export to JSON failed.
```

## 8.3 PowerShell Channels

Logging should map to native PowerShell output channels:

```text
Verbose logs -> WriteVerbose
Debug logs -> WriteDebug
Warnings -> WriteWarning
Errors -> WriteError
```

Operational logs should respect `-Verbose`.

---

# 9. Error Handling Specification

All error handling must be centralized.

Required types:

```text
InfisicalException
InfisicalApiException
InfisicalAuthenticationException
InfisicalHttpException
InfisicalSerializationException
InfisicalExportException
InfisicalConfigurationException
InfisicalErrorDetails
InfisicalErrorHandler
```

## 9.1 Error Details

Errors should preserve:

```text
Component
Operation
Message
Exception type
Inner exception message
HTTP status code
HTTP reason phrase
API error code when available
Sanitized API error body when safe
JSON line number when available
JSON position when available
Request endpoint key
Request method
```

## 9.2 Error Logging

When a failure occurs, log multiple sanitized details where possible:

```text
[UTC] - [Error] - [ErrorHandler] - Operation failed: RetrieveSecret
[UTC] - [Error] - [ErrorHandler] - Error Component: SecretsClient
[UTC] - [Error] - [ErrorHandler] - Error Message: The Infisical API returned Forbidden.
[UTC] - [Error] - [ErrorHandler] - HTTP Status Code: 403
[UTC] - [Error] - [ErrorHandler] - API Error Code: <if available>
[UTC] - [Error] - [ErrorHandler] - Line: <if available>
[UTC] - [Error] - [ErrorHandler] - Position: <if available>
```

## 9.3 PowerShell Error Records

Cmdlets must emit proper `ErrorRecord` objects.

Examples:

```text
CategoryInfo:
  AuthenticationError
  ConnectionError
  InvalidData
  InvalidOperation
  PermissionDenied
  ResourceUnavailable
  WriteError
```

The real underlying error must bubble up, but sanitized to avoid exposing secrets.

---

# 10. Endpoint Registry

All endpoint definitions must be centralized.

No cmdlet may hard-code endpoint paths.

## 10.1 Endpoint Definition Model

```csharp
public sealed class InfisicalEndpointDefinition
{
    public string Name { get; set; }
    public string Resource { get; set; }
    public string Version { get; set; }
    public string Method { get; set; }
    public string Template { get; set; }
    public bool RequiresAuthorization { get; set; }
    public bool ContainsSecretMaterialInRequest { get; set; }
    public bool ContainsSecretMaterialInResponse { get; set; }
}
```

## 10.2 Initial Endpoint Definitions

```text
UniversalAuthLogin:
  Method: POST
  Version: v1
  Template: /api/v1/auth/universal-auth/login
  RequiresAuthorization: false
  ContainsSecretMaterialInRequest: true
  ContainsSecretMaterialInResponse: true

ListSecrets:
  Method: GET
  Version: v4
  Template: /api/v4/secrets
  RequiresAuthorization: true
  ContainsSecretMaterialInRequest: false
  ContainsSecretMaterialInResponse: true

RetrieveSecret:
  Method: GET
  Version: v4
  Template: /api/v4/secrets/{secretName}
  RequiresAuthorization: true
  ContainsSecretMaterialInRequest: false
  ContainsSecretMaterialInResponse: true
```

Universal Auth uses a client ID and client secret to obtain an access token, and Infisical documents the login endpoint as `/api/v1/auth/universal-auth/login`. ([Infisical Blog][2])

## 10.3 API Version Flexibility

`Connect-Infisical` should accept:

```powershell
-ApiVersion
```

Default:

```text
v4
```

However, API version must not be assumed globally for every resource. The endpoint registry must allow each endpoint family to specify its own version.

---

# 11. URI and Path Handling

## 11.1 URI Rules

All URLs must use:

```csharp
System.Uri
System.UriBuilder
```

URI construction must be centralized in:

```text
InfisicalUriBuilder
```

Responsibilities:

```text
Combine base URI and endpoint path.
Escape path segments.
Escape query parameters.
Support repeated query parameters.
Avoid manual string concatenation.
Preserve scheme/host/port.
Support Linux, macOS, and Windows.
```

## 11.2 Path Rules

Internal filesystem paths must use:

```csharp
System.IO.FileInfo
System.IO.DirectoryInfo
System.IO.Path.Combine(...)
```

PowerShell build/helper scripts must use:

```powershell
[System.IO.FileInfo][System.IO.Path]::Combine(...)
[System.IO.DirectoryInfo][System.IO.Path]::Combine(...)
```

Public command examples can use simple strings because PowerShell can bind strings to `FileInfo`, `DirectoryInfo`, and `Uri`.

Public example:

```powershell
Export-InfisicalSecrets -Format Env -Path '.\secrets.env'
```

Internal implementation must still use proper typed path handling.

---

# 12. Authentication Design

## 12.1 Supported Auth Types

Currently implemented:

```text
Universal Auth
Token Auth
JWT Auth
OIDC Auth
LDAP Auth
Azure Auth
GCP IAM Auth
```

Each implemented provider is exposed as a dedicated `Connect-Infisical` parameter set. Identity-based providers (JWT, OIDC, Azure, GCP IAM) share a common login flow via `IdentityLoginExecutor` and POST to `/api/v1/auth/{provider}-auth/login`. Infisical documents identity authentication modes such as Universal Auth and Token Auth for API access, and API interaction requires an access token. ([Infisical Blog][3])

## 12.2 Future Auth Types

Design must allow future support for:

```text
AWS IAM Auth
Kubernetes Auth
TLS Certificate Auth
Alibaba Cloud Auth
OCI Auth
```

These should not be exposed as public parameter sets until actually implemented.

## 12.3 Auth Provider Interface

```csharp
public interface IInfisicalAuthProvider
{
    string Name { get; }

    InfisicalAuthenticationResult Authenticate(InfisicalAuthenticationRequest request, IInfisicalHttpClient httpClient, IInfisicalLogger logger);
}
```

## 12.4 Authentication Result

```csharp
public sealed class InfisicalAuthenticationResult
{
    public SecureString AccessToken { get; set; }
    public DateTimeOffset? ExpiresAtUtc { get; set; }
    public string TokenType { get; set; }
}
```

`AccessToken` must be read-only.

---

# 13. Connection Management

The module must maintain a process-level current connection.

## 13.1 Session Manager

```csharp
public static class InfisicalSessionManager
{
    public static InfisicalConnection Current { get; }

    public static void SetCurrent(InfisicalConnection connection);

    public static InfisicalConnection RequireCurrent();

    public static void Disconnect();
}
```

## 13.2 Connection Model

```csharp
public sealed class InfisicalConnection
{
    public Uri BaseUri { get; set; }
    public string ApiVersion { get; set; }
    public InfisicalAuthType AuthType { get; set; }
    public string OrganizationId { get; set; }
    public string ProjectId { get; set; }
    public string Environment { get; set; }
    public string DefaultSecretPath { get; set; }
    public DateTimeOffset ConnectedAtUtc { get; set; }
    public DateTimeOffset? ExpiresAtUtc { get; set; }
    public bool IsConnected { get; set; }

    internal SecureString AccessToken { get; set; }
}
```

The public object must not display or serialize `AccessToken`.

---

# 14. Public Cmdlet Specifications

# 14.1 Connect-Infisical

## Purpose

Authenticate to Infisical and store the current connection.

## Approved Verb

```text
Connect
```

## Parameter Sets

### Universal Auth

```powershell
Connect-Infisical `
    -BaseUri <Uri> `
    -OrganizationId <string> `
    -ProjectId <string> `
    -Environment <string> `
    -ClientId <string> `
    -ClientSecret <SecureString> `
    [-SecretPath <string>] `
    [-ApiVersion <string>] `
    [-PassThru]
```

### Token Auth

```powershell
Connect-Infisical `
    -BaseUri <Uri> `
    -OrganizationId <string> `
    -ProjectId <string> `
    -Environment <string> `
    -AccessToken <SecureString> `
    [-SecretPath <string>] `
    [-ApiVersion <string>] `
    [-PassThru]
```

## Defaults

```text
SecretPath: /
ApiVersion: v4
```

## Behavior

```text
Validate BaseUri.
Validate ProjectId.
Validate Environment.
Validate OrganizationId when provided.
Validate ApiVersion.
Authenticate if using Universal Auth.
Store returned access token internally.
Make access token SecureString read-only.
Create InfisicalConnection.
Store connection in InfisicalSessionManager.
Return connection only when -PassThru is used.
```

## Example

```powershell
$ClientSecret = Read-Host -Prompt 'Client Secret' -AsSecureString

Connect-Infisical `
    -BaseUri 'https://app.infisical.com' `
    -OrganizationId 'organization-id' `
    -ProjectId 'project-id' `
    -Environment 'prod' `
    -ClientId 'client-id' `
    -ClientSecret $ClientSecret `
    -SecretPath '/' `
    -Verbose
```

## Token Example

```powershell
$Token = Read-Host -Prompt 'Access Token' -AsSecureString

Connect-Infisical `
    -BaseUri 'https://app.infisical.com' `
    -OrganizationId 'organization-id' `
    -ProjectId 'project-id' `
    -Environment 'prod' `
    -AccessToken $Token
```

---

# 14.2 Disconnect-Infisical

## Purpose

Disconnect the current Infisical session.

## Approved Verb

```text
Disconnect
```

## Parameters

```powershell
Disconnect-Infisical [-PassThru]
```

## Behavior

```text
Clear current connection.
Dispose/clear token references where practical.
Clear cached authentication metadata.
Return nothing by default.
Return disconnected status object when -PassThru is used.
```

## Example

```powershell
Disconnect-Infisical -Verbose
```

---

# 14.3 Get-InfisicalSecrets

## Purpose

Retrieve a list of secrets.

The Infisical list secrets endpoint supports listing from a base path and can recursively fetch subdirectories up to the documented depth limit. It also supports values such as `viewSecretValue`, `expandSecretReferences`, and `recursive`. ([Infisical Blog][4])

## Approved Verb

```text
Get
```

## Parameters

```powershell
Get-InfisicalSecrets `
    [-ProjectId <string>] `
    [-Environment <string>] `
    [-SecretPath <string>] `
    [-Recursive] `
    [-IncludeImports] `
    [-IncludePersonalOverrides] `
    [-ExpandSecretReferences] `
    [-ViewSecretValue] `
    [-MetadataFilter <hashtable>] `
    [-TagSlugs <string[]>]
```

## Defaults

```text
ProjectId: Current connection ProjectId
Environment: Current connection Environment
SecretPath: Current connection DefaultSecretPath or /
Recursive: false
IncludeImports: false
ExpandSecretReferences: false
ViewSecretValue: true
```

## Behavior

```text
Require active connection.
Use explicit ProjectId/Environment/SecretPath when supplied.
Use connection defaults otherwise.
Build URI centrally.
Call ListSecrets endpoint.
Parse response into typed models.
Convert secretValue to read-only SecureString immediately.
Clear temporary response models where practical.
Return InfisicalSecret objects.
```

## Example

```powershell
Get-InfisicalSecrets -SecretPath '/production/web' -Recursive -Verbose
```

---

# 14.4 Get-InfisicalSecret

## Purpose

Retrieve a single secret by name.

Infisical documents the retrieve-secret endpoint as `/api/v4/secrets/{secretName}`. ([Infisical Blog][5])

## Approved Verb

```text
Get
```

## Parameters

```powershell
Get-InfisicalSecret `
    -SecretName <string> `
    [-ProjectId <string>] `
    [-Environment <string>] `
    [-SecretPath <string>] `
    [-Version <int>] `
    [-Type <InfisicalSecretType>] `
    [-ViewSecretValue] `
    [-ExpandSecretReferences] `
    [-IncludeImports]
```

## Parameter Attributes

```text
SecretName:
  Mandatory
  ValueFromPipelineByPropertyName
```

## Defaults

```text
ProjectId: Current connection ProjectId
Environment: Current connection Environment
SecretPath: Current connection DefaultSecretPath or /
Type: Shared
ViewSecretValue: true
ExpandSecretReferences: false
IncludeImports: false
```

## Behavior

```text
Require active connection.
Escape SecretName as path segment.
Build query centrally.
Call RetrieveSecret endpoint.
Parse response into typed model.
Convert secretValue to read-only SecureString immediately.
Return one InfisicalSecret object.
```

## Example

```powershell
Get-InfisicalSecret -SecretName 'SqlPassword' -SecretPath '/production/sql'
```

Pipeline example:

```powershell
[pscustomobject]@{ SecretName = 'SqlPassword' } | Get-InfisicalSecret -SecretPath '/production/sql'
```

---

# 14.5 ConvertTo-InfisicalSecretDictionary

## Purpose

Convert one or more `InfisicalSecret` objects to a case-insensitive dictionary.

## Approved Verb

```text
ConvertTo
```

## Parameters

```powershell
ConvertTo-InfisicalSecretDictionary `
    -InputObject <InfisicalSecret[]> `
    [-DuplicateKeyBehavior <Error|FirstWins|LastWins>]
```

## Parameter Attributes

```text
InputObject:
  Mandatory
  ValueFromPipeline
```

## Default

```text
DuplicateKeyBehavior: Error
```

## Return Type

```csharp
Dictionary<string, SecureString>
```

Comparer:

```csharp
StringComparer.OrdinalIgnoreCase
```

## Behavior

```text
Key = SecretName.
Value = SecretValue as SecureString.
Do not convert values to plaintext.
Throw on duplicate keys by default.
Support FirstWins and LastWins when explicitly selected.
```

## Example

```powershell
$Secrets = Get-InfisicalSecrets -SecretPath '/production'
$Dictionary = $Secrets | ConvertTo-InfisicalSecretDictionary
```

---

# 14.6 Export-InfisicalSecrets

## Purpose

Export secrets to:

```text
JSON
YAML
ENV
XML
EnvironmentVariables
```

## Approved Verb

```text
Export
```

## Parameters

```powershell
Export-InfisicalSecrets `
    -InputObject <InfisicalSecret[]> `
    -Format <Json|Yaml|Env|Xml|EnvironmentVariables> `
    [-Path <FileInfo>] `
    [-Scope <Process|User|Machine>] `
    [-Force] `
    [-Encoding <UTF8|UTF8Bom|Unicode>] `
    [-Prefix <string>]
```

## Parameter Rules

```text
InputObject:
  Mandatory
  ValueFromPipeline

Format:
  Mandatory

Path:
  Mandatory for Json, Yaml, Env, Xml
  Not required for EnvironmentVariables

Scope:
  Only used for EnvironmentVariables
  Default: Process

Encoding:
  Default: UTF8
```

## Behavior

```text
Collect pipeline input.
Use centralized exporter provider.
Create Path.Directory automatically when missing.
Use FileInfo.Directory directly.
Do not emit warning messages.
Do not log exported values.
Do not write secret plaintext to verbose output.
Use InfisicalSecret.UsePlainTextValue() for scoped plaintext conversion.
```

## Example

```powershell
Get-InfisicalSecrets -SecretPath '/production' | Export-InfisicalSecrets -Format Json -Path '.\secrets.json'
```

```powershell
Get-InfisicalSecrets -SecretPath '/production' | Export-InfisicalSecrets -Format Env -Path '.\secrets.env'
```

```powershell
Get-InfisicalSecrets -SecretPath '/production' | Export-InfisicalSecrets -Format EnvironmentVariables -Scope Process
```

---

# 15. Output Models

## 15.1 InfisicalSecret

```csharp
public sealed class InfisicalSecret
{
    public string Id { get; set; }
    public string InternalId { get; set; }
    public string Workspace { get; set; }
    public string Environment { get; set; }
    public int? Version { get; set; }
    public InfisicalSecretType Type { get; set; }
    public string SecretName { get; set; }
    public SecureString SecretValue { get; set; }
    public bool SecretValueHidden { get; set; }
    public string SecretPath { get; set; }
    public string SecretComment { get; set; }
    public DateTimeOffset? CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
    public bool IsRotatedSecret { get; set; }
    public Guid? RotationId { get; set; }
    public InfisicalSecretTag[] Tags { get; set; }
    public InfisicalSecretMetadata[] SecretMetadata { get; set; }

    public T UsePlainTextValue<T>(Func<string, T> action)
    {
        return SecureStringUtility.UsePlainText(SecretValue, action);
    }

    public void UsePlainTextValue(Action<string> action)
    {
        SecureStringUtility.UsePlainText(SecretValue, plainText =>
        {
            action(plainText);
            return true;
        });
    }

    public override string ToString()
    {
        return SecretName;
    }
}
```

## 15.2 Rules

```text
No PlainTextValue property.
No public getter that returns plaintext.
No secret value in ToString().
No secret value in default display formatting.
No secret value in logs.
Plaintext access only through scoped method.
Exporters must use UsePlainTextValue().
```

## 15.3 InfisicalSecretMetadata

```csharp
public sealed class InfisicalSecretMetadata
{
    public string Key { get; set; }
    public string Value { get; set; }
}
```

Only unencrypted metadata should be exported.

## 15.4 InfisicalSecretTag

```csharp
public sealed class InfisicalSecretTag
{
    public string Id { get; set; }
    public string Slug { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
}
```

## 15.5 Enums

```csharp
public enum InfisicalSecretType
{
    Shared,
    Personal
}
```

```csharp
public enum InfisicalExportFormat
{
    Json,
    Yaml,
    Env,
    Xml,
    EnvironmentVariables
}
```

```csharp
public enum InfisicalDuplicateKeyBehavior
{
    Error,
    FirstWins,
    LastWins
}
```

---

# 16. Export Format Specifications

## 16.1 ENV

Format:

```text
Key=Value
```

Example:

```text
SqlServer=192.168.1.10
SqlUser=appuser
SqlPassword=ExamplePassword
```

## 16.2 XML

Required schema:

```xml
<Secrets>
  <Secret>
    <SecretName></SecretName>
    <SecretValue></SecretValue>
    <SecretPath></SecretPath>
    <SecretMetadata>
      <Metadata>
        <Key></Key>
        <Value></Value>
      </Metadata>
    </SecretMetadata>
  </Secret>
</Secrets>
```

## 16.3 JSON

JSON should be an array of secret objects.

Example:

```json
[
  {
    "SecretName": "production",
    "SecretValue": "192.168.1.10",
    "SecretPath": "/servers",
    "SecretMetadata": {
      "Owner": "Infrastructure"
    }
  },
  {
    "SecretName": "staging",
    "SecretValue": "192.168.1.12",
    "SecretPath": "/servers",
    "SecretMetadata": {
      "Owner": "Infrastructure"
    }
  }
]
```

## 16.4 YAML

Required shape:

```yaml
Secrets:
  - SecretName: production
    SecretValue: 192.168.1.10
    SecretPath: /servers
    SecretMetadata:
      Owner: Infrastructure
  - SecretName: staging
    SecretValue: 192.168.1.12
    SecretPath: /servers
    SecretMetadata:
      Owner: Infrastructure
```

## 16.5 EnvironmentVariables

Use:

```csharp
Environment.SetEnvironmentVariable(name, value, target);
```

Supported scopes:

```text
Process
User
Machine
```

Default:

```text
Process
```

No warnings should be emitted.

---

# 16.6 Start-InfisicalProcess

Signature:

```text
Start-InfisicalProcess
    -FilePath <string>
    [-WorkingDirectory <DirectoryInfo>]
    [-ArgumentList <string[]>]
    [-AcceptableExitCodeList <string[]>]
    [-WindowStyle <Normal|Hidden|Minimized|Maximized>]
    [-CreateNoWindow]
    [-NoWait]
    [-Priority <AboveNormal|BelowNormal|High|Idle|Normal|RealTime>]
    [-ExecutionTimeout <TimeSpan>]
    [-ExecutionTimeoutInterval <TimeSpan>]
    [-StandardInputObjectList <object[]>]
    [-EnvironmentVariables <IDictionary>]
    [-ParsingExpression <Regex>]
    [-SecureArgumentList]
    [-LogOutput]
    [-ContinueOnError]
    [-Secret <InfisicalSecret[]>]
    [-Prefix <string>]
```

Behavior:

```text
Buffer pipeline InfisicalSecret objects in ProcessRecord.
Decrypt secrets only into ProcessStartInfo.Environment.
Apply -Prefix to each secret name before injection.
Never write secret plaintext to user or machine environment scope.
Honor -WhatIf / -Confirm.
Default -AcceptableExitCodeList = @('0','3010').
Throw a terminating error on unacceptable exit code unless -ContinueOnError is set.
```

Output: `InfisicalProcessResult` with `ExitCode`, `ExitCodeAsHex`, `ExitCodeAsInteger`, `ExitCodeAsDecimal`, `StandardOutput`, `StandardError`, `StandardOutputObject`, `StandardErrorObject`, `StartTime`, `ExitTime`, `Duration`, `DurationFriendly`, `ProcessId`, `TimedOut`, `Succeeded`, `SecretCount`.

---

# 16.7 Organization Cmdlets

Organizations are the top-level tenancy boundary in Infisical. They are not scoped under a project; the active connection's `OrganizationId` is used as the default identifier when an explicit one is not supplied.

Cmdlet signatures:

```powershell
Get-InfisicalOrganization [[-OrganizationId] <string>]                          # default = List
New-InfisicalOrganization [-Name] <string> [-Slug <string>] [-WhatIf] [-Confirm]
Update-InfisicalOrganization [-OrganizationId] <string> [-Name <string>] [-Slug <string>] [-WhatIf] [-Confirm]
Remove-InfisicalOrganization [-OrganizationId] <string> [-PassThru] [-WhatIf] [-Confirm]
```

Parameter sets:

| Cmdlet | Default set | Single set | Notes |
|---|---|---|---|
| `Get-InfisicalOrganization` | `List` (no `-Id`) | `Single` (`-OrganizationId`/`-Id`) | No `-ProjectId`. |
| `New-InfisicalOrganization` | n/a | `-Name` mandatory, `-Slug` optional | ShouldProcess. |
| `Update-InfisicalOrganization` | n/a | `-OrganizationId` mandatory | ShouldProcess; only bound parameters are sent. |
| `Remove-InfisicalOrganization` | n/a | `-OrganizationId` mandatory | `ConfirmImpact.High`; `-PassThru` emits removed id. |

Endpoints:

| Operation | Method | Template | Version |
|---|---|---|---|
| List | `GET` | `/api/v2/organizations` | v2 |
| Retrieve | `GET` | `/api/v1/organization/{organizationId}` | v1 |
| Create | `POST` | `/api/v2/organizations` | v2 |
| Update | `PATCH` | `/api/v1/organization/{organizationId}` | v1 |
| Delete | `DELETE` | `/api/v1/organization/{organizationId}` | v1 |

Output: `InfisicalOrganization` with `Id`, `Name`, `Slug`, `CustomerId`, `AuthEnforced`, `ScimEnabled`, `CreatedAtUtc`, `UpdatedAtUtc`.

---

# 16.8 Sub-Organization Cmdlets

Sub-organizations partition an organization into isolated child tenants. They are not scoped under a project; the active connection is used for the parent organization context.

Cmdlet signatures:

```powershell
Get-InfisicalSubOrganization [[-SubOrganizationId] <string>] [-Limit <int>] [-Offset <int>] [-Search <string>] [-OrderBy <string>] [-OrderDirection <string>] [-IsAccessible]
New-InfisicalSubOrganization [-Name] <string> [-Slug] <string> [-WhatIf] [-Confirm]
Update-InfisicalSubOrganization [-SubOrganizationId] <string> [-Name <string>] [-Slug <string>] [-WhatIf] [-Confirm]
Remove-InfisicalSubOrganization [-SubOrganizationId] <string> [-PassThru] [-WhatIf] [-Confirm]
```

Parameter sets:

| Cmdlet | Default set | Single set | Notes |
|---|---|---|---|
| `Get-InfisicalSubOrganization` | `List` (no `-Id`) | `Single` (`-SubOrganizationId`/`-Id`) | List supports server-side `-Limit`, `-Offset`, `-Search`, `-OrderBy`, `-OrderDirection`, `-IsAccessible`. |
| `New-InfisicalSubOrganization` | n/a | `-Name` + `-Slug` mandatory | ShouldProcess. |
| `Update-InfisicalSubOrganization` | n/a | `-SubOrganizationId` mandatory | ShouldProcess; only bound parameters are sent. |
| `Remove-InfisicalSubOrganization` | n/a | `-SubOrganizationId` mandatory | `ConfirmImpact.High`; `-PassThru` emits removed id. |

Endpoints (beta):

| Operation | Method | Template | Version |
|---|---|---|---|
| List | `GET` | `/api/v1/sub-organizations` | v1 |
| Retrieve | `GET` | `/api/v1/sub-organizations/{subOrgId}` | v1 |
| Create | `POST` | `/api/v1/sub-organizations` | v1 |
| Update | `PATCH` | `/api/v1/sub-organizations/{subOrgId}` | v1 |
| Delete | `DELETE` | `/api/v1/sub-organizations/{subOrgId}` | v1 |

Output: `InfisicalSubOrganization` with `Id`, `Name`, `Slug`, `OrganizationId`, `IsAccessible`, `CreatedAtUtc`, `UpdatedAtUtc`.

---

# 17. SecureString Utility

Required utility:

```csharp
public static class SecureStringUtility
{
    public static SecureString ToReadOnlySecureString(string value)
    {
        SecureString secureString = new SecureString();

        if (!string.IsNullOrEmpty(value))
        {
            foreach (char character in value)
            {
                secureString.AppendChar(character);
            }
        }

        secureString.MakeReadOnly();

        return secureString;
    }

    public static T UsePlainText<T>(SecureString secureString, Func<string, T> action)
    {
        if (secureString == null)
        {
            throw new ArgumentNullException(nameof(secureString));
        }

        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        IntPtr pointer = IntPtr.Zero;

        try
        {
            pointer = Marshal.SecureStringToBSTR(secureString);
            string plainText = Marshal.PtrToStringBSTR(pointer);
            return action(plainText);
        }
        finally
        {
            if (pointer != IntPtr.Zero)
            {
                Marshal.ZeroFreeBSTR(pointer);
            }
        }
    }
}
```

Rules:

```text
Use SecureString for credentials.
Use SecureString for secret output values.
Call MakeReadOnly() after population.
Do not reuse mutable SecureString values unless necessary.
Do not expose SecureString internals.
Do not log conversion failures with secret content.
```

---

# 18. HTTP Client Design

## 18.1 Interface

```csharp
public interface IInfisicalHttpClient
{
    InfisicalHttpResponse Send(InfisicalHttpRequest request);
}
```

## 18.2 Request Model

```csharp
public sealed class InfisicalHttpRequest
{
    public string OperationName { get; set; }
    public string EndpointName { get; set; }
    public string Method { get; set; }
    public Uri Uri { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public string Body { get; set; }
    public bool ContainsSecretMaterialInRequest { get; set; }
    public bool ContainsSecretMaterialInResponse { get; set; }
}
```

## 18.3 Response Model

```csharp
public sealed class InfisicalHttpResponse
{
    public int StatusCode { get; set; }
    public string ReasonPhrase { get; set; }
    public string Body { get; set; }
    public Dictionary<string, string> Headers { get; set; }

    public void Clear()
    {
        Body = null;
        Headers?.Clear();
    }
}
```

## 18.4 Rules

```text
Use synchronous request execution.
Do not log Authorization header.
Do not log request body when ContainsSecretMaterialInRequest is true.
Do not log response body when ContainsSecretMaterialInResponse is true.
Clear response body as soon as parsed.
```

---

# 19. Serialization Design

Required serializers:

```text
IInfisicalSerializer
JsonInfisicalSerializer
YamlInfisicalSerializer
XmlInfisicalSerializer
EnvInfisicalSerializer
EnvironmentVariableExporter
```

Recommended packages:

```text
Newtonsoft.Json
YamlDotNet
```

Responsibilities:

```text
Serialize strongly typed export models.
Deserialize API responses.
Preserve line/position details where available.
Avoid logging raw secret-bearing data.
Clear temporary DTOs where practical.
```

---

# 20. DTO Design

API DTOs should be separate from public output models.

Example:

```text
InfisicalSecretResponseDto
InfisicalSecretListResponseDto
InfisicalUniversalAuthLoginRequestDto
InfisicalUniversalAuthLoginResponseDto
```

Rules:

```text
DTOs may temporarily hold plaintext from API responses.
DTOs must not be returned publicly.
DTOs should be cleared/released after mapping.
Mapping must convert secretValue to read-only SecureString immediately.
```

---

# 21. Mapping Design

Required mapper:

```text
InfisicalSecretMapper
```

Responsibilities:

```text
Map secretKey to SecretName.
Map secretValue to SecureString.
Map secretPath.
Map environment.
Map metadata.
Map tags.
Map timestamps to UTC DateTimeOffset.
Clear DTO secretValue after mapping where practical.
```

Example mapping behavior:

```text
DTO.secretKey -> InfisicalSecret.SecretName
DTO.secretValue -> InfisicalSecret.SecretValue
DTO.secretPath -> InfisicalSecret.SecretPath
DTO.secretMetadata -> InfisicalSecret.SecretMetadata
```

---

# 22. Documentation Requirements

Each public cmdlet must have help and examples.

Docs required:

```text
about_PSInfisicalAPI.help.txt
Connect-Infisical.md
Disconnect-Infisical.md
Get-InfisicalSecrets.md
Get-InfisicalSecret.md
ConvertTo-InfisicalSecretDictionary.md
Export-InfisicalSecrets.md
```

Each cmdlet help page must include:

```text
Synopsis
Description
Parameters
Inputs
Outputs
Examples
Notes
```

Public examples should be clean and natural.

Example:

```powershell
Connect-Infisical -BaseUri 'https://app.infisical.com' -OrganizationId 'org-id' -ProjectId 'project-id' -Environment 'prod' -ClientId 'client-id' -ClientSecret $ClientSecret
```

Do not force examples to show internal `FileInfo` or `Path.Combine` usage.

---

# 23. Type and Format Data

The module should include formatting so secrets display safely.

Default view for `InfisicalSecret`:

```text
SecretName
SecretPath
Environment
Type
Version
UpdatedAtUtc
SecretValueHidden
```

Do not display:

```text
SecretValue
AccessToken
ClientSecret
RawApiResponse
```

---

# 24. Testing Requirements

## 24.1 Unit Tests

Minimum tests:

```text
Build script is idempotent.
Manifest version matches assembly version.
Manifest commit hash exists.
PSM1 imports binary from bin folder.
Endpoint registry returns ListSecrets endpoint.
Endpoint registry returns RetrieveSecret endpoint.
Endpoint registry returns UniversalAuthLogin endpoint.
URI builder escapes query parameters.
URI builder escapes secret name path segment.
Logger does not log secret values.
Logger formats UTC timestamp correctly.
Error handler preserves HTTP status code.
Error handler sanitizes secret-bearing response content.
SecureStringUtility creates read-only SecureString.
InfisicalSecret.ToString() returns SecretName only.
InfisicalSecret.UsePlainTextValue() scopes plaintext access.
Get-InfisicalSecrets maps secretKey to SecretName.
Get-InfisicalSecrets maps secretValue to SecureString.
ConvertTo-InfisicalSecretDictionary uses OrdinalIgnoreCase.
ConvertTo-InfisicalSecretDictionary throws on duplicate by default.
Export JSON creates directory automatically.
Export YAML creates directory automatically.
Export XML matches required schema.
Export ENV writes Key=Value.
EnvironmentVariables export defaults to Process.
No warning emitted during export.
```

## 24.2 Integration Tests

Integration tests require:

```text
CLOUDINIT_INFISICAL_APIURL
CLOUDINIT_INFISICAL_ORGANIZATIONID
CLOUDINIT_INFISICAL_PROJECTID
CLOUDINIT_INFISICAL_ENVIRONMENT
CLOUDINIT_INFISICAL_CLIENTID
CLOUDINIT_INFISICAL_CLIENTSECRET
```

Integration tests must verify:

```text
Connect-Infisical works with Universal Auth.
Get-InfisicalSecrets returns typed objects.
Get-InfisicalSecret returns one typed object.
SecretValue is SecureString.
SecretValue is read-only.
Export formats complete without warning.
Disconnect-Infisical clears current session.
```

---

# 25. Implementation Milestones

## Milestone 1: Skeleton

```text
Repository structure
C# project
Test project
Initial build.ps1
Initial PSD1 generation
Initial PSM1 generation
Version generation
Commit hash embedding
CHANGELOG.md
```

## Milestone 2: Core Infrastructure

```text
Central logger
Central error types
Central error handler
SecureString utility
Path utility
Endpoint registry
URI builder
Sanitizer
```

## Milestone 3: HTTP and Serialization

```text
Synchronous HTTP client
HTTP request/response models
JSON serializer
YAML serializer
XML serializer
ENV serializer
Response clearing behavior
```

## Milestone 4: Connection and Auth

```text
Connection model
Session manager
Auth provider interface
Universal Auth provider
Token Auth provider
Connect-Infisical
Disconnect-Infisical
```

## Milestone 5: Secrets

```text
Secret DTOs
Secret output models
Secret mapper
Get-InfisicalSecrets
Get-InfisicalSecret
Safe formatting
```

## Milestone 6: Conversion and Export

```text
ConvertTo-InfisicalSecretDictionary
JSON export
YAML export
XML export
ENV export
EnvironmentVariables export
No-warning export behavior
```

## Milestone 7: Tests and Docs

```text
Unit tests
Integration test harness
External help
README examples
CHANGELOG update
Import validation in Windows PowerShell 5.1
Import validation in PowerShell 7+
```

---

# 26. Acceptance Criteria

The project is acceptable only when all of these are true:

```text
The module is named PSInfisicalAPI.
The module is C# based.
The module targets .NET Standard 2.0.
The module works in Windows PowerShell 5.1.
The module works in PowerShell 7+.
No async keyword exists in source.
No await keyword exists in source.
All public cmdlets use approved verbs.
All public cmdlets have help.
All public cmdlets have examples.
Connect-Infisical supports Universal Auth.
Connect-Infisical supports Token Auth.
Disconnect-Infisical clears the current connection.
Other cmdlets automatically use the current connection.
Endpoint paths are centralized.
URI building is centralized.
Query construction is centralized.
All URLs use System.Uri or UriBuilder.
All internal paths use FileInfo, DirectoryInfo, and Path.Combine.
Logging is centralized.
Logging uses [UTC Timestamp] - [Level] - [Component] - Message.
Operations log before and after.
Failures log sanitized error detail.
Secret values are never logged.
Client secrets are never logged.
Access tokens are never logged.
Authorization headers are never logged.
Secret API response bodies are never logged.
Public cmdlets never return raw API responses.
Secret values are stored as SecureString.
SecureString values are made read-only where practical.
Secret object has scoped plaintext conversion method.
Secret object has no plaintext property.
Exporters use scoped plaintext conversion.
Export operations do not emit warning messages.
Export JSON works.
Export YAML works.
Export ENV works.
Export XML works.
Export EnvironmentVariables works.
EnvironmentVariables export supports Process, User, and Machine.
EnvironmentVariables export defaults to Process.
Export path uses FileInfo.
Export creates missing directories.
ConvertTo-InfisicalSecretDictionary returns case-insensitive dictionary.
Duplicate dictionary keys error by default.
Version format is yyyy.MM.dd.HHmm.
Commit hash is embedded.
Build script is idempotent.
Build script generates manifest.
Build script copies binaries to Module/PSInfisicalAPI/bin.
Build script creates release folders.
Build script can run unit tests.
Build script can run integration tests only when explicitly requested.
Integration tests read machine-scope CLOUDINIT_INFISICAL_* variables.
Successful milestones are committed when -CommitOnSuccess is used.
Failed builds are never committed.
```

---

# 27. Final Design Principle

`PSInfisicalAPI` should not be a one-off secrets script wrapped in a module. It should be a reusable, strongly typed, secure framework for Infisical API interaction, with secrets as the first supported resource family and future support for certificates, identities, projects, folders, imports, rotations, and other Infisical API areas added cleanly through new endpoint definitions, models, services, and cmdlets.

[1]: https://infisical.com/docs/api-reference/overview/introduction?utm_source=chatgpt.com "API Reference"
[2]: https://infisical.com/docs/documentation/platform/identities/universal-auth?utm_source=chatgpt.com "Universal Auth"
[3]: https://infisical.com/docs/api-reference/overview/authentication?utm_source=chatgpt.com "Authentication"
[4]: https://infisical.com/docs/api-reference/endpoints/secrets/list?utm_source=chatgpt.com "List secrets"
[5]: https://infisical.com/docs/api-reference/endpoints/secrets/read?utm_source=chatgpt.com "Retrieve"