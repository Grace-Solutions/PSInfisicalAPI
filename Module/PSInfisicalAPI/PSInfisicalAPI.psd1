@{
    RootModule           = 'PSInfisicalAPI.psm1'
    ModuleVersion        = '2026.06.03.0057'
    GUID                 = 'b8a2f3d4-7c51-4d2f-9e6a-1f0c8b3d4e51'
    Author               = 'Grace Solutions'
    CompanyName          = 'Grace Solutions'
    Copyright            = '(c) Grace Solutions. All rights reserved.'
    Description          = 'PSInfisicalAPI is a C# binary PowerShell module for the Infisical REST API, providing cmdlets for authentication, secret retrieval, and export with automatic environment-variable discovery across Process, User, and Machine scopes.'
    PowerShellVersion    = '5.1'
    CompatiblePSEditions = @('Desktop','Core')
    FunctionsToExport    = @()
    CmdletsToExport      = @(
        'Connect-Infisical',
        'Disconnect-Infisical',
        'Get-InfisicalSecrets',
        'Get-InfisicalSecret',
        'ConvertTo-InfisicalSecretDictionary',
        'Export-InfisicalSecrets'
    )
    AliasesToExport      = @()
    VariablesToExport    = @()
    FormatsToProcess     = @('PSInfisicalAPI.Format.ps1xml')
    TypesToProcess       = @('PSInfisicalAPI.Types.ps1xml')
    PrivateData          = @{
        PSData = @{
            Tags         = @('Infisical','Secrets','API','SecureString','Vault','Authentication')
            LicenseUri   = 'https://www.gnu.org/licenses/agpl-3.0.html'
            ProjectUri   = 'https://prod.git.gracesolution.info/gsadmin/PSInfisicalAPI'
            ReleaseNotes = 'See CHANGELOG.md in the project repository for release history.'
            CommitHash   = '7e5209190ac2'
        }
    }
}