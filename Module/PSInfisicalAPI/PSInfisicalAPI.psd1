@{
    RootModule           = 'PSInfisicalAPI.psm1'
    ModuleVersion        = '2026.06.02.1638'
    GUID                 = 'b8a2f3d4-7c51-4d2f-9e6a-1f0c8b3d4e51'
    Author               = 'Alphaeus Mote'
    CompanyName          = ''
    Copyright            = '(c) Alphaeus Mote. All rights reserved.'
    Description          = 'PSInfisicalAPI is a C# binary PowerShell module for the Infisical REST API.'
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
            Tags        = @('Infisical','Secrets','API','SecureString')
            ProjectUri  = ''
            ReleaseNotes = ''
            CommitHash  = '3c47d6ff30ec'
        }
    }
}