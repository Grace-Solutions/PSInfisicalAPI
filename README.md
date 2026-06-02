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

| Cmdlet                                | Purpose                                                                    |
| ------------------------------------- | -------------------------------------------------------------------------- |
| `Connect-Infisical`                   | Establish a session using Universal Auth or a pre-issued access token.     |
| `Disconnect-Infisical`                | Clear the current session.                                                 |
| `Get-InfisicalSecrets`                | List secrets at a given path / environment.                                |
| `Get-InfisicalSecret`                 | Retrieve a single secret by name.                                          |
| `ConvertTo-InfisicalSecretDictionary` | Convert secret objects into a `Hashtable` keyed by `SecretKey`.            |
| `Export-InfisicalSecrets`             | Export secrets to JSON, YAML, XML, or `.env` format.                       |

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

Get-InfisicalSecrets -SecretPath '/'
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
Get-InfisicalSecrets
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

## Continuous integration

`.gitea/workflows/publish-psgallery.yml` publishes the module to the PowerShell Gallery whenever a pull request is merged into `main`. The workflow expects a repository secret named `PSGALLERY_API_KEY` containing a valid Gallery API key.

## License

Distributed under the GNU Affero General Public License v3.0. See [LICENSE](LICENSE).
