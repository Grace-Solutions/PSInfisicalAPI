[CmdletBinding()]
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

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$RepositoryRoot = [System.IO.DirectoryInfo]$PSScriptRoot
$SrcRoot        = [System.IO.DirectoryInfo][System.IO.Path]::Combine($RepositoryRoot.FullName, 'src')
$ProjectFile    = [System.IO.FileInfo][System.IO.Path]::Combine($SrcRoot.FullName, 'PSInfisicalAPI', 'PSInfisicalAPI.csproj')
$TestsFile      = [System.IO.FileInfo][System.IO.Path]::Combine($SrcRoot.FullName, 'PSInfisicalAPI.Tests', 'PSInfisicalAPI.Tests.csproj')
$ModuleRoot     = [System.IO.DirectoryInfo][System.IO.Path]::Combine($RepositoryRoot.FullName, 'Module', 'PSInfisicalAPI')
$ModuleBinDir   = [System.IO.DirectoryInfo][System.IO.Path]::Combine($ModuleRoot.FullName, 'bin')
$ArtifactsDir   = [System.IO.DirectoryInfo][System.IO.Path]::Combine($RepositoryRoot.FullName, 'Artifacts')
$ReleasesDir    = [System.IO.DirectoryInfo][System.IO.Path]::Combine($RepositoryRoot.FullName, 'Releases')
$ChangelogFile  = [System.IO.FileInfo][System.IO.Path]::Combine($RepositoryRoot.FullName, 'CHANGELOG.md')
$ModuleGuid     = 'b8a2f3d4-7c51-4d2f-9e6a-1f0c8b3d4e51'

function Write-Step {
    param([string]$Message)
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Get-BuildVersion {
    return (Get-Date).ToUniversalTime().ToString('yyyy.MM.dd.HHmm')
}

function Get-CommitHash {
    try {
        $hash = (& git rev-parse --short=12 HEAD 2>$null).Trim()
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrEmpty($hash)) {
            return 'unknown'
        }

        return $hash
    } catch {
        return 'unknown'
    }
}

function Ensure-Directory {
    param([System.IO.DirectoryInfo]$Directory)
    if (-not $Directory.Exists) { [void]$Directory.Create() }
}

function Clear-Directory {
    param([System.IO.DirectoryInfo]$Directory)
    if ($Directory.Exists) {
        Get-ChildItem -LiteralPath $Directory.FullName -Force | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    }
}

function Get-AssemblyVersion {
    param([string]$BuildVersion)
    $parts = $BuildVersion -split '\.'
    if ($parts.Length -ne 4) { return '1.0.0.0' }
    $year   = [int]$parts[0]
    $month  = [int]$parts[1]
    $day    = [int]$parts[2]
    $hhmm   = [int]$parts[3]
    $assemblyMinor = ($month * 100) + $day
    return ("{0}.{1}.{2}.0" -f $year, $assemblyMinor, $hhmm)
}

function Write-Manifest {
    param(
        [System.IO.FileInfo]$Path,
        [string]$ModuleVersion,
        [string]$CommitHash
    )

    $content = @"
@{
    RootModule           = 'PSInfisicalAPI.psm1'
    ModuleVersion        = '$ModuleVersion'
    GUID                 = '$ModuleGuid'
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
        'Remove-InfisicalTag',
        'Get-InfisicalCertificateAuthority',
        'Search-InfisicalCertificate',
        'ConvertTo-InfisicalCertificate',
        'Install-InfisicalCertificate',
        'Uninstall-InfisicalCertificate',
        'Export-InfisicalCertificate'
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
            CommitHash   = '$CommitHash'
        }
    }
}
"@

    [System.IO.File]::WriteAllText($Path.FullName, $content, [System.Text.UTF8Encoding]::new($false))
}

function Update-Changelog {
    param([string]$Version, [string]$CommitHash)

    if (-not $ChangelogFile.Exists) { return }
    $marker = "## $Version"
    $existing = Get-Content -LiteralPath $ChangelogFile.FullName -Raw
    if ($existing -match [Regex]::Escape($marker)) { return }

    $insertion = "## $Version`r`n`r`n- Build produced from commit $CommitHash.`r`n`r`n"
    $unreleasedRegex = [regex]::new('(?m)^## Unreleased\r?$')
    if (-not $unreleasedRegex.IsMatch($existing)) { return }
    $updated = $unreleasedRegex.Replace($existing, "## Unreleased`r`n`r`n$insertion## Unreleased (carried forward)", 1)
    [System.IO.File]::WriteAllText($ChangelogFile.FullName, $updated, [System.Text.UTF8Encoding]::new($false))
}


function Invoke-DotNet {
    param([string[]]$Arguments)
    Write-Step ("dotnet " + ($Arguments -join ' '))
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command failed: $($Arguments -join ' ')"
    }
}

function Test-ModuleImports {
    param([System.IO.DirectoryInfo]$ModuleDirectory)
    Write-Step "Validating module import, manifest, and help"
    $manifestPath = [System.IO.Path]::Combine($ModuleDirectory.FullName, 'PSInfisicalAPI.psd1')
    $script = @"
`$ErrorActionPreference = 'Stop'

`$manifest = Test-ModuleManifest -Path '$manifestPath'
if (`$null -eq `$manifest) {
    throw "Test-ModuleManifest returned no result for '$manifestPath'."
}

Import-Module -Name '$($ModuleDirectory.FullName)' -Force

`$cmds = @('Connect-Infisical','Disconnect-Infisical','Get-InfisicalSecrets','Get-InfisicalSecret','New-InfisicalSecret','Update-InfisicalSecret','Remove-InfisicalSecret','ConvertTo-InfisicalSecretDictionary','Export-InfisicalSecrets','Get-InfisicalProjects','Get-InfisicalProject','New-InfisicalProject','Update-InfisicalProject','Remove-InfisicalProject','Get-InfisicalEnvironments','Get-InfisicalEnvironment','New-InfisicalEnvironment','Update-InfisicalEnvironment','Remove-InfisicalEnvironment','Get-InfisicalFolders','Get-InfisicalFolder','New-InfisicalFolder','Update-InfisicalFolder','Remove-InfisicalFolder','Get-InfisicalTags','Get-InfisicalTag','New-InfisicalTag','Update-InfisicalTag','Remove-InfisicalTag','Get-InfisicalCertificateAuthority','Search-InfisicalCertificate','ConvertTo-InfisicalCertificate','Install-InfisicalCertificate','Uninstall-InfisicalCertificate','Export-InfisicalCertificate')
foreach (`$c in `$cmds) {
    if (-not (Get-Command -Name `$c -Module PSInfisicalAPI -ErrorAction SilentlyContinue)) {
        throw "Cmdlet not found: `$c"
    }

    `$help = Get-Help -Name `$c -ErrorAction SilentlyContinue
    if (`$null -eq `$help) {
        throw "Get-Help returned nothing for cmdlet: `$c"
    }
}

`$about = Get-Help -Name 'about_PSInfisicalAPI' -ErrorAction SilentlyContinue
if (`$null -eq `$about -or [string]::IsNullOrWhiteSpace((`$about | Out-String))) {
    throw "Get-Help 'about_PSInfisicalAPI' returned no content. Ensure en-US/about_PSInfisicalAPI.help.txt is present."
}
"@

    $tempFile = [System.IO.FileInfo][System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [System.IO.Path]::GetRandomFileName() + '.ps1')
    try {
        [System.IO.File]::WriteAllText($tempFile.FullName, $script, [System.Text.UTF8Encoding]::new($false))
        & pwsh -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $tempFile.FullName
        if ($LASTEXITCODE -ne 0) { throw "Module import validation failed in pwsh." }
    } finally {
        if ($tempFile.Exists) { Remove-Item -LiteralPath $tempFile.FullName -Force -ErrorAction SilentlyContinue }
    }
}

$buildVersion    = Get-BuildVersion
$assemblyVersion = Get-AssemblyVersion -BuildVersion $buildVersion
$commitHash      = Get-CommitHash

Write-Step "Build version: $buildVersion"
Write-Step "Assembly version: $assemblyVersion"
Write-Step "Commit hash: $commitHash"

Ensure-Directory -Directory $ArtifactsDir
Ensure-Directory -Directory $ModuleRoot
Ensure-Directory -Directory $ModuleBinDir

if ($Clean.IsPresent) {
    Write-Step "Cleaning generated outputs"
    Clear-Directory -Directory $ModuleBinDir
    Clear-Directory -Directory $ArtifactsDir
    Get-ChildItem -LiteralPath $SrcRoot.FullName -Recurse -Directory -Force |
        Where-Object { $_.Name -in @('bin','obj') } |
        ForEach-Object { Remove-Item -LiteralPath $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }
}

if ($Restore.IsPresent) {
    Invoke-DotNet -Arguments @('restore', $ProjectFile.FullName)
    Invoke-DotNet -Arguments @('restore', $TestsFile.FullName)
}

Write-Step "Building $($ProjectFile.Name) ($Configuration)"
$buildArgs = @(
    'build', $ProjectFile.FullName,
    '-c', $Configuration,
    '--nologo',
    "-p:BuildVersion=$buildVersion",
    "-p:BuildAssemblyVersion=$assemblyVersion",
    "-p:BuildCommitHash=$commitHash"
)
Invoke-DotNet -Arguments $buildArgs

if ($RunTests.IsPresent -or $RunIntegrationTests.IsPresent) {
    Write-Step "Running tests"
    $testFilter = if ($RunIntegrationTests.IsPresent) { 'Category!=NEVER' } else { 'Category!=Integration' }
    $testArgs = @(
        'test', $TestsFile.FullName,
        '-c', $Configuration,
        '--nologo',
        '--filter', $testFilter,
        "-p:BuildVersion=$buildVersion",
        "-p:BuildAssemblyVersion=$assemblyVersion",
        "-p:BuildCommitHash=$commitHash"
    )
    Invoke-DotNet -Arguments $testArgs
}

Write-Step "Publishing module binaries"
$publishOutput = [System.IO.DirectoryInfo][System.IO.Path]::Combine($ArtifactsDir.FullName, 'publish')
Ensure-Directory -Directory $publishOutput
Clear-Directory -Directory $publishOutput
$publishArgs = @(
    'publish', $ProjectFile.FullName,
    '-c', $Configuration,
    '--nologo',
    '-o', $publishOutput.FullName,
    "-p:BuildVersion=$buildVersion",
    "-p:BuildAssemblyVersion=$assemblyVersion",
    "-p:BuildCommitHash=$commitHash"
)
Invoke-DotNet -Arguments $publishArgs

Clear-Directory -Directory $ModuleBinDir
$desiredAssemblies = @('PSInfisicalAPI.dll','Newtonsoft.Json.dll','YamlDotNet.dll','BouncyCastle.Cryptography.dll')
foreach ($assembly in $desiredAssemblies) {
    $source = [System.IO.FileInfo][System.IO.Path]::Combine($publishOutput.FullName, $assembly)
    if ($source.Exists) {
        Copy-Item -LiteralPath $source.FullName -Destination $ModuleBinDir.FullName -Force
    }
}

$manifestPath = [System.IO.FileInfo][System.IO.Path]::Combine($ModuleRoot.FullName, 'PSInfisicalAPI.psd1')
Write-Manifest -Path $manifestPath -ModuleVersion $buildVersion -CommitHash $commitHash

Update-Changelog -Version $buildVersion -CommitHash $commitHash

Test-ModuleImports -ModuleDirectory $ModuleRoot

if ($CreateRelease.IsPresent) {
    $releaseDir = [System.IO.DirectoryInfo][System.IO.Path]::Combine($ReleasesDir.FullName, $buildVersion)
    if ($releaseDir.Exists -and -not $Force.IsPresent) {
        throw "Release '$buildVersion' already exists. Pass -Force to overwrite."
    }

    Ensure-Directory -Directory $ReleasesDir
    if ($releaseDir.Exists) { Clear-Directory -Directory $releaseDir }
    Ensure-Directory -Directory $releaseDir

    $releaseModuleDir = [System.IO.DirectoryInfo][System.IO.Path]::Combine($releaseDir.FullName, 'PSInfisicalAPI')
    Ensure-Directory -Directory $releaseModuleDir
    Copy-Item -LiteralPath ([System.IO.Path]::Combine($ModuleRoot.FullName, '*')) -Destination $releaseModuleDir.FullName -Recurse -Force
    Write-Step "Release created at $($releaseDir.FullName)"
}

if ($CommitOnSuccess.IsPresent) {
    Write-Step "Committing on success"
    & git add -A
    if ($LASTEXITCODE -ne 0) { throw "git add failed." }
    & git commit -m "Build $buildVersion"
    if ($LASTEXITCODE -ne 0) { throw "git commit failed." }
}

Write-Step "Build complete."
