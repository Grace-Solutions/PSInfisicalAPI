# PSInfisicalAPI

A C# binary PowerShell module for interacting with the [Infisical](https://infisical.com/) REST API. It provides cmdlets for authentication, secret retrieval, structured export, and includes automatic environment-variable discovery so connections can be established with little or no inline configuration.

- License: AGPL-3.0
- Author: Grace Solutions
- Target framework: .NET Standard 2.0 (compatible with Windows PowerShell 5.1 and PowerShell 7+)

## Installation

### From the PowerShell Gallery

```powershell
Install-Module -Name PSInfisicalAPI -Scope CurrentUser
Import-Module -Name PSInfisicalAPI
```

### From source

```powershell
git clone https://prod.git.gracesolution.info/gsadmin/PSInfisicalAPI.git
cd PSInfisicalAPI
pwsh -NoProfile -ExecutionPolicy Bypass -File .\build.ps1 -RunTests
Import-Module -Name .\Module\PSInfisicalAPI
```

## Cmdlets

The module exports 34 cmdlets. Discovery cmdlets (`Get-Infisical*`) use a `List` (default) / single-record parameter-set pair: invoking without the identity parameter returns the collection, supplying the identity parameter returns one record.

| Area                  | Cmdlets                                                                                                                                                                                          |
| --------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Session               | `Connect-Infisical`, `Disconnect-Infisical`                                                                                                                                                      |
| Secrets               | `Get-InfisicalSecret`, `New-InfisicalSecret`, `Update-InfisicalSecret`, `Remove-InfisicalSecret`, `Copy-InfisicalSecret`, `ConvertTo-InfisicalSecretDictionary`, `Export-InfisicalSecrets`       |
| Projects              | `Get-InfisicalProject`, `New-InfisicalProject`, `Update-InfisicalProject`, `Remove-InfisicalProject`                                                                                             |
| Environments          | `Get-InfisicalEnvironment`, `New-InfisicalEnvironment`, `Update-InfisicalEnvironment`, `Remove-InfisicalEnvironment`                                                                             |
| Folders               | `Get-InfisicalFolder`, `New-InfisicalFolder`, `Update-InfisicalFolder`, `Remove-InfisicalFolder`                                                                                                 |
| Tags                  | `Get-InfisicalTag`, `New-InfisicalTag`, `Update-InfisicalTag`, `Remove-InfisicalTag`                                                                                                             |
| PKI                   | `Get-InfisicalCertificateAuthority`, `Get-InfisicalPkiSubscriber`, `Get-InfisicalCertificate`, `Search-InfisicalCertificate`, `Request-InfisicalCertificate`, `ConvertTo-InfisicalCertificate`, `Install-InfisicalCertificate`, `Uninstall-InfisicalCertificate`, `Export-InfisicalCertificate` |

Use `Get-Help <Cmdlet> -Full` for parameter details and `Get-Help about_PSInfisicalAPI` for the module overview.

## Quick start

```powershell
$secureSecret = Read-Host -AsSecureString 'Client Secret'

$connection = Connect-Infisical `
    -BaseUri        'https://app.infisical.com' `
    -OrganizationId '00000000-0000-0000-0000-000000000000' `
    -ProjectId      '11111111-1111-1111-1111-111111111111' `
    -Environment    'dev' `
    -ClientId       'machine-identity-client-id' `
    -ClientSecret   $secureSecret `
    -PassThru

Get-InfisicalSecret -SecretPath '/'
Disconnect-Infisical
```

## Automatic environment-variable discovery

When `Connect-Infisical` is invoked with one or more parameters missing (or set to whitespace/empty), the cmdlet searches environment variables and uses the first value it finds. This makes invocation as simple as `Connect-Infisical` when variables are set up in advance.

### Scope precedence

Scopes are searched in order; the first matching variable with a non-blank value wins:

1. `Process`
2. `User`
3. `Machine`

### Patterns

The resolver matches case-insensitively against patterns aligned with Infisical's CLI defaults plus common variants such as `CLOUDINIT_INFISICAL_*` and custom-prefixed names (e.g., `myapp_infisical_client_id`).

| Parameter         | Example variable names matched                                                       |
| ----------------- | ------------------------------------------------------------------------------------ |
| `BaseUri`         | `INFISICAL_API_URL`, `INFISICAL_BASE_URL`, `INFISICAL_HOST`                          |
| `OrganizationId`  | `INFISICAL_ORG_ID`, `INFISICAL_ORGANIZATION_ID`                                      |
| `ProjectId`       | `INFISICAL_PROJECT_ID`, `INFISICAL_WORKSPACE_ID`                                     |
| `Environment`     | `INFISICAL_ENVIRONMENT`, `INFISICAL_ENV`, `INFISICAL_ENV_SLUG`                       |
| `ClientId`        | `INFISICAL_CLIENT_ID`, `INFISICAL_UNIVERSAL_AUTH_CLIENT_ID`                          |
| `ClientSecret`    | `INFISICAL_CLIENT_SECRET`, `INFISICAL_UNIVERSAL_AUTH_CLIENT_SECRET`                  |
| `AccessToken`     | `INFISICAL_TOKEN`, `INFISICAL_ACCESS_TOKEN`, `INFISICAL_AUTH_TOKEN`                  |
| `SecretPath`      | `INFISICAL_SECRET_PATH`, `INFISICAL_DEFAULT_SECRET_PATH`                             |
| `ApiVersion`      | `INFISICAL_API_VERSION`                                                              |

Sensitive values (`ClientSecret`, `AccessToken`) are read directly into a read-only `SecureString` and never logged.

### Zero-configuration example

```powershell
[Environment]::SetEnvironmentVariable('INFISICAL_API_URL',       'https://app.infisical.com',          'User')
[Environment]::SetEnvironmentVariable('INFISICAL_ORG_ID',        '00000000-0000-0000-0000-000000000000', 'User')
[Environment]::SetEnvironmentVariable('INFISICAL_PROJECT_ID',    '11111111-1111-1111-1111-111111111111', 'User')
[Environment]::SetEnvironmentVariable('INFISICAL_ENVIRONMENT',   'dev',                                  'User')
[Environment]::SetEnvironmentVariable('INFISICAL_CLIENT_ID',     'machine-identity-client-id',           'User')
[Environment]::SetEnvironmentVariable('INFISICAL_CLIENT_SECRET', 'super-secret-value',                   'User')

Connect-Infisical
Get-InfisicalSecret
```

### Mixed example (explicit values override discovery)

Explicit parameters always win over discovered values; blank/whitespace explicit values trigger discovery.

```powershell
Connect-Infisical -Environment 'prod'   # everything else discovered from environment
```

### Logging

The resolver emits a single verbose line announcing the scan and one informational line per discovered variable (variable name and scope; values are never logged). Use `-Verbose` to see the scan announcement.

## Building

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\build.ps1 -RunTests
```

The script builds the binary, runs unit tests, publishes binaries into `Module/PSInfisicalAPI/bin/`, regenerates the manifest, and validates that the module imports.

## Extending the module

### Adding a new API endpoint

All HTTP routes live in two files under `src/PSInfisicalAPI/Endpoints/`:

- `InfisicalEndpointNames.cs` declares a `const string` identifier for each endpoint.
- `InfisicalEndpointRegistry.cs` maps each identifier to one or more `InfisicalEndpointDefinition` records grouped by resource (`RegisterAuthentication`, `RegisterSecrets`, `RegisterPki`, etc.).

To add a route:

1. Add a constant in `InfisicalEndpointNames.cs` (e.g., `public const string ListPkiSubscribers = "ListPkiSubscribers";`).
2. In the matching `Register<Resource>` method, call `Add(map, new InfisicalEndpointDefinition { ... })` with `Name`, `Resource`, `Version`, `Method`, `Template`, and the `RequiresAuthorization` / `ContainsSecretMaterialInRequest` / `ContainsSecretMaterialInResponse` flags. Use `{placeholder}` tokens in `Template`; they are substituted from the `pathParameters` dictionary passed by the caller.
3. If the same logical operation has more than one upstream path (legacy + current), register both definitions under the same `Name` — `InvokeWithCandidateFallback` tries each in order until one succeeds.
4. Invoke the endpoint from the appropriate client (`InfisicalPkiClient`, `InfisicalSecretsClient`, etc.) via `_invoker.InvokeWithCandidateFallback(connection, InfisicalEndpointNames.XYZ, "XYZ", pathParameters, query, body)`.

### Adding a new cmdlet

Cmdlets live in `src/PSInfisicalAPI/Cmdlets/` and derive from `InfisicalCmdletBase`, which exposes `HttpClient`, `Logger`, `ResolveProjectId`, and `ThrowTerminatingForException`. Follow the consolidated discovery pattern when the cmdlet supports both list and single-record retrieval:

```csharp
[Cmdlet(VerbsCommon.Get, "InfisicalPkiSubscriber", DefaultParameterSetName = "List")]
[OutputType(typeof(InfisicalPkiSubscriber))]
public sealed class GetInfisicalPkiSubscriberCmdlet : InfisicalCmdletBase
{
    [Parameter(ParameterSetName = "ByName", Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
    [Alias("SubscriberName", "Slug")]
    public string Name { get; set; }

    [Parameter] public string ProjectId { get; set; }

    protected override void ProcessRecord() { /* dispatch on ParameterSetName */ }
}
```

After adding (or removing) a cmdlet:

1. Update `build.ps1` in **two** places — the `CmdletsToExport` array inside the generated manifest block, and the `$expectedCmds` array used by `Test-ModuleImports`. Both must list the same cmdlets; the build fails fast if they drift.
2. Add a `<command:command>` entry in `Module/PSInfisicalAPI/en-US/PSInfisicalAPI.dll-Help.xml`. Each entry must include a non-empty `<maml:description>` synopsis (do not let it start with the cmdlet name — the validation gate rejects PowerShell's auto-generated fallback), a non-empty `<maml:description>` body, and at least one `<command:example>` with a non-empty `<dev:code>` block.
3. For consolidated `List` / single-record cmdlets, ship **three examples**: two straight-line invocations (one per parameter set) and one `OrderedDictionary` splat. The splat must construct the dictionary with `OrdinalIgnoreCase` so parameter names round-trip case-insensitively:

   ```powershell
   $Params = New-Object -TypeName 'System.Collections.Specialized.OrderedDictionary' -ArgumentList ([System.StringComparer]::OrdinalIgnoreCase)
   $Params.ProjectId = (Get-InfisicalProject | Select-Object -First 1).Id
   $Result = Get-InfisicalPkiSubscriber @Params
   ```
4. Add a `## Unreleased` entry to `CHANGELOG.md` describing the change (mark removals of public cmdlets or parameters as **BREAKING**).
5. Run `./build.ps1 -RunTests`. The script enforces the cmdlet list, runs the xUnit suite, and verifies that every exported cmdlet has a valid synopsis, description, and at least one non-empty example.

## Continuous integration

`.gitea/workflows/publish-psgallery.yml` publishes the module to the PowerShell Gallery whenever a pull request is merged into `main`. The workflow expects a repository secret named `PSGALLERY_API_KEY` containing a valid Gallery API key.

## License

Distributed under the GNU Affero General Public License v3.0. See [LICENSE](LICENSE).
