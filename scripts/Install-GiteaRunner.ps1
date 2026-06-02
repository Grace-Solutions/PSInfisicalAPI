#Requires -Version 7.0
<#
.SYNOPSIS
    Downloads the Gitea act_runner binary, installs it as a system daemon, and registers it.

.DESCRIPTION
    Cross-platform installer for the Gitea Actions runner. Idempotent: re-running upgrades
    in place, skips work when the requested version already matches, and reuses an existing
    registration unless -Force is supplied.

    Resolution order for InstanceUrl and RegistrationToken when not passed explicitly:
        Process scope env vars -> User scope env vars -> Machine scope env vars
    Explicit candidate variable names tried in order:
        InstanceUrl:      GITEA_INSTANCE_URL, GITEA_URL,
                          CLOUDINIT_GITEA_INSTANCE_URL, CLOUDINIT_GITEA_URL
        RegistrationToken: GITEA_RUNNER_REGISTRATION_TOKEN, GITEA_RUNNER_TOKEN,
                           GITEA_REGISTRATION_TOKEN,
                           CLOUDINIT_GITEA_RUNNER_REGISTRATION_TOKEN,
                           CLOUDINIT_GITEA_RUNNER_TOKEN,
                           CLOUDINIT_GITEA_REGISTRATION_TOKEN

.PARAMETER InstanceUrl
    Base URL of the Gitea instance (e.g. https://prod.git.gracesolution.info).

.PARAMETER RegistrationToken
    Runner registration token. Treated as secret; never logged.

.PARAMETER RunnerName
    Name presented to Gitea. Defaults to the machine hostname.

.PARAMETER Labels
    Optional comma-separated runner labels (e.g. linux-amd64,docker). When omitted,
    no --labels argument is passed to act_runner register and the runner is
    registered with whatever labels are configured server-side or in config.yaml.

.PARAMETER Version
    Specific release tag (e.g. v1.0.7) or 'latest'. Defaults to latest.

.PARAMETER InstallRoot
    Override directory. Defaults are /opt/gitea/runner on Linux,
    /usr/local/gitea/runner on macOS, and %ProgramData%\Gitea\Runner on Windows.

.PARAMETER BinaryName
    Renamed binary on disk. Defaults to act_runner (act_runner.exe on Windows).

.PARAMETER ServiceName
    System service identifier (used as the systemd unit name, launchd label suffix,
    and Windows service name). Must not contain spaces or characters illegal in
    a unit/service identifier. Defaults to 'gitea-runner'.

.PARAMETER ServiceDisplayName
    Friendly human-readable name shown in systemctl status, Windows Services MMC,
    and similar UIs. Defaults to 'Gitea Runner'.

.PARAMETER Force
    Re-download binary and re-register the runner even when already present.

.EXAMPLE
    pwsh -File Install-GiteaRunner.ps1 -InstanceUrl https://prod.git.gracesolution.info

.EXAMPLE
    Remote one-liner using env vars for InstanceUrl / RegistrationToken:
    $env:GITEA_INSTANCE_URL = 'https://prod.git.gracesolution.info'
    $env:GITEA_RUNNER_REGISTRATION_TOKEN = '<token>'
    irm 'https://prod.git.gracesolution.info/gsadmin/Gitea-Bootstrap/raw/branch/main/scripts/Install-GiteaRunner.ps1' | iex
#>
[CmdletBinding()]
param(
    [string] $InstanceUrl,
    [string] $RegistrationToken,
    [string] $RunnerName = [System.Net.Dns]::GetHostName(),
    [string] $Labels,
    [string] $Version = 'latest',
    [string] $InstallRoot,
    [string] $BinaryName,
    [string] $ServiceName = 'gitea-runner',
    [string] $ServiceDisplayName = 'Gitea Runner',
    [switch] $Force
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

if ($PSVersionTable.PSVersion.Major -lt 7) {
    throw "Install-GiteaRunner requires PowerShell 7 or later. Detected: $($PSVersionTable.PSVersion)."
}

$script:Utf8NoBom = New-Object System.Text.UTF8Encoding($false)

function Write-Stage {
    param([string] $Message)
    $stamp = [DateTimeOffset]::UtcNow.UtcDateTime.ToString('yyyy-MM-ddTHH:mm:ss.fffffffZ')
    Write-Host "[$stamp] - [Install-GiteaRunner] - $Message"
}

function Resolve-FromEnvVarNames {
    param(
        [Parameter(Mandatory)][string[]] $Names,
        [string] $DisplayName
    )

    $scopes = @('Process', 'User', 'Machine')
    foreach ($name in $Names) {
        foreach ($scope in $scopes) {
            try { $value = [System.Environment]::GetEnvironmentVariable($name, [System.EnvironmentVariableTarget]::$scope) }
            catch { continue }

            if (-not [string]::IsNullOrWhiteSpace($value)) {
                Write-Stage "Resolved $DisplayName from $scope env var '$name'."
                return [string] $value
            }
        }
    }
    return $null
}

function Get-PlatformDescriptor {
    $arch = [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString().ToLowerInvariant()
    switch ($arch) {
        'x64'   { $assetArch = 'amd64' }
        'arm64' { $assetArch = 'arm64' }
        'arm'   { $assetArch = 'arm-7' }
        default { throw "Unsupported architecture: $arch" }
    }

    if ($IsWindows) { return @{ Os = 'windows'; Arch = $assetArch; Suffix = '.exe' } }
    if ($IsMacOS)   { return @{ Os = 'darwin';  Arch = $assetArch; Suffix = ''     } }
    if ($IsLinux)   { return @{ Os = 'linux';   Arch = $assetArch; Suffix = ''     } }
    throw 'Unable to determine host operating system.'
}

function Get-DefaultInstallRoot {
    if ($IsWindows) { return (Join-Path $env:ProgramData 'Gitea' 'Runner') }
    if ($IsMacOS)   { return '/usr/local/gitea/runner' }
    return '/opt/gitea/runner'
}

function Assert-Elevated {
    if ($IsWindows) {
        $principal = New-Object Security.Principal.WindowsPrincipal ([Security.Principal.WindowsIdentity]::GetCurrent())
        if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
            throw 'This script must be run from an elevated PowerShell session (Administrator).'
        }
    } else {
        $uid = & id -u
        if ($uid -ne '0') { throw 'This script must be run as root (use sudo).' }
    }
}

function Resolve-ReleaseTag {
    param([string] $Requested)
    if ($Requested -and $Requested -ne 'latest') { return ($Requested.TrimStart('v')) }

    Write-Stage 'Querying Gitea for latest runner release.'
    $proxyUri = [System.Net.WebRequest]::GetSystemWebProxy().GetProxy([Uri] 'https://gitea.com')
    $invokeArgs = @{
        Uri                  = 'https://gitea.com/api/v1/repos/gitea/act_runner/releases/latest'
        UseBasicParsing      = $true
        UseDefaultCredentials = $true
    }
    if ($proxyUri -and $proxyUri.AbsoluteUri -ne 'https://gitea.com/') {
        $invokeArgs.Proxy = $proxyUri.AbsoluteUri
        $invokeArgs.ProxyUseDefaultCredentials = $true
    }
    $release = Invoke-RestMethod @invokeArgs
    if (-not $release.tag_name) { throw 'Failed to resolve latest act_runner release tag.' }
    return ($release.tag_name.TrimStart('v'))
}


function Get-InstalledBinaryVersion {
    param([string] $BinaryPath)
    if (-not (Test-Path -LiteralPath $BinaryPath)) { return $null }
    try {
        $raw = & $BinaryPath --version 2>$null
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($raw)) { return $null }
        $match = [regex]::Match($raw, '(\d+\.\d+\.\d+(?:[\-\.][0-9A-Za-z\.\-]+)?)')
        if ($match.Success) { return $match.Groups[1].Value }
    } catch { }
    return $null
}

function Save-BinaryViaWebClient {
    param(
        [Parameter(Mandatory)][string] $Uri,
        [Parameter(Mandatory)][string] $Destination
    )
    Write-Stage "Downloading $Uri"
    $client = New-Object System.Net.WebClient
    try {
        $client.Headers['User-Agent'] = 'Install-GiteaRunner'
        $client.UseDefaultCredentials = $true
        $proxy = [System.Net.WebRequest]::GetSystemWebProxy()
        $proxy.Credentials = [System.Net.CredentialCache]::DefaultNetworkCredentials
        $client.Proxy = $proxy
        $client.DownloadFile($Uri, $Destination)
    } finally {
        $client.Dispose()
    }
}

function Add-ToMachinePath {
    param([Parameter(Mandatory)][string] $Directory)
    if ($IsWindows) {
        $current = [System.Environment]::GetEnvironmentVariable('Path', 'Machine')
        $parts = @()
        if ($current) { $parts = $current.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries) }
        if ($parts -notcontains $Directory) {
            $updated = ($parts + $Directory) -join ';'
            [System.Environment]::SetEnvironmentVariable('Path', $updated, 'Machine')
            Write-Stage "Added $Directory to Machine PATH."
        }
        if (-not ($env:Path -split ';' -contains $Directory)) { $env:Path = "$env:Path;$Directory" }
        return
    }

    $profilePath = '/etc/profile.d/gitea-runner.sh'
    $line = "export PATH=`"$Directory`":`$PATH"
    $existing = if ([System.IO.File]::Exists($profilePath)) { [System.IO.File]::ReadAllText($profilePath) } else { '' }
    if ($existing -notmatch [regex]::Escape($Directory)) {
        [System.IO.File]::WriteAllText($profilePath, $line, $script:Utf8NoBom)
        & chmod 0644 $profilePath | Out-Null
        Write-Stage "Wrote $profilePath for system-wide PATH."
    }
    if (($env:PATH -split ':') -notcontains $Directory) { $env:PATH = "$Directory`:$env:PATH" }
}

function Register-Runner {
    param(
        [Parameter(Mandatory)][string] $BinaryPath,
        [Parameter(Mandatory)][string] $WorkingDirectory,
        [Parameter(Mandatory)][string] $InstanceUrl,
        [Parameter(Mandatory)][string] $Token,
        [Parameter(Mandatory)][string] $RunnerName,
        [string] $Labels,
        [switch] $Force
    )

    $configPath = Join-Path $WorkingDirectory 'config.yaml'
    if ((-not [System.IO.File]::Exists($configPath)) -or $Force) {
        Write-Stage 'Generating config.yaml.'
        Push-Location $WorkingDirectory
        try {
            $configContent = (& $BinaryPath generate-config) | Out-String
            [System.IO.File]::WriteAllText($configPath, $configContent, $script:Utf8NoBom)
        } finally { Pop-Location }
    }

    $runnerStateFile = Join-Path $WorkingDirectory '.runner'
    if ((Test-Path -LiteralPath $runnerStateFile) -and -not $Force) {
        Write-Stage 'Runner already registered (.runner present); skipping registration.'
        return
    }

    $labelDescription = if ([string]::IsNullOrWhiteSpace($Labels)) { '(no labels specified; using server/config defaults)' } else { "with labels '$Labels'" }
    Write-Stage "Registering runner '$RunnerName' $labelDescription."

    Push-Location $WorkingDirectory
    try {
        $registerArgs = @('register', '--no-interactive', '--instance', $InstanceUrl,
                          '--token', $Token, '--name', $RunnerName, '--config', $configPath)
        if (-not [string]::IsNullOrWhiteSpace($Labels)) { $registerArgs += @('--labels', $Labels) }
        & $BinaryPath @registerArgs
        if ($LASTEXITCODE -ne 0) { throw "act_runner register exited with code $LASTEXITCODE." }
    } finally { Pop-Location }
}

function Install-SystemdUnit {
    param(
        [Parameter(Mandatory)][string] $ServiceName,
        [Parameter(Mandatory)][string] $ServiceDisplayName,
        [Parameter(Mandatory)][string] $BinaryPath,
        [Parameter(Mandatory)][string] $WorkingDirectory
    )
    $unitPath = "/etc/systemd/system/$ServiceName.service"
    $configPath = Join-Path $WorkingDirectory 'config.yaml'
    $unit = @"
[Unit]
Description=$ServiceDisplayName
After=network-online.target
Wants=network-online.target

[Service]
Type=simple
WorkingDirectory=$WorkingDirectory
ExecStart=$BinaryPath daemon --config $configPath
Restart=always
RestartSec=5
User=root

[Install]
WantedBy=multi-user.target
"@
    [System.IO.File]::WriteAllText($unitPath, $unit, $script:Utf8NoBom)
    & systemctl daemon-reload | Out-Null
    & systemctl enable $ServiceName | Out-Null
    & systemctl restart $ServiceName | Out-Null
    Write-Stage "Systemd unit $unitPath installed and started."
}

function Install-LaunchdPlist {
    param(
        [Parameter(Mandatory)][string] $ServiceName,
        [Parameter(Mandatory)][string] $ServiceDisplayName,
        [Parameter(Mandatory)][string] $BinaryPath,
        [Parameter(Mandatory)][string] $WorkingDirectory
    )
    $label = "com.gitea.$ServiceName"
    $plistPath = "/Library/LaunchDaemons/$label.plist"
    $configPath = Join-Path $WorkingDirectory 'config.yaml'
    $plist = @"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key><string>$label</string>
    <key>ProgramArguments</key>
    <array>
        <string>$BinaryPath</string>
        <string>daemon</string>
        <string>--config</string>
        <string>$configPath</string>
    </array>
    <key>WorkingDirectory</key><string>$WorkingDirectory</string>
    <key>RunAtLoad</key><true/>
    <key>KeepAlive</key><true/>
</dict>
</plist>
"@
    [System.IO.File]::WriteAllText($plistPath, $plist, $script:Utf8NoBom)
    & chmod 0644 $plistPath | Out-Null
    & launchctl unload $plistPath 2>$null | Out-Null
    & launchctl load -w $plistPath | Out-Null
    Write-Stage "Launchd plist $plistPath installed and loaded."
}

function Install-WindowsService {
    param(
        [Parameter(Mandatory)][string] $ServiceName,
        [Parameter(Mandatory)][string] $ServiceDisplayName,
        [Parameter(Mandatory)][string] $BinaryPath,
        [Parameter(Mandatory)][string] $WorkingDirectory
    )
    $configPath = Join-Path $WorkingDirectory 'config.yaml'
    $binPath = "`"$BinaryPath`" daemon --config `"$configPath`""
    $existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($existing) {
        Write-Stage "Service $ServiceName exists; updating binary path and restarting."
        & sc.exe config $ServiceName binPath= $binPath DisplayName= $ServiceDisplayName | Out-Null
        if ($existing.Status -eq 'Running') { Stop-Service -Name $ServiceName -Force }
    } else {
        Write-Stage "Creating Windows service $ServiceName."
        & sc.exe create $ServiceName binPath= $binPath start= auto DisplayName= $ServiceDisplayName | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "sc.exe create failed with exit code $LASTEXITCODE." }
    }
    Start-Service -Name $ServiceName
    Write-Stage "Service $ServiceName started."
}


# --- Main ----------------------------------------------------------------

Assert-Elevated

if ([string]::IsNullOrWhiteSpace($ServiceName) -or ($ServiceName -match '\s')) {
    throw "ServiceName must be a non-empty identifier with no whitespace (got: '$ServiceName'). Use -ServiceDisplayName for the friendly label."
}
if ([string]::IsNullOrWhiteSpace($ServiceDisplayName)) { $ServiceDisplayName = $ServiceName }

if ([string]::IsNullOrWhiteSpace($InstanceUrl)) {
    $InstanceUrl = Resolve-FromEnvVarNames -DisplayName 'InstanceUrl' -Names @(
        'GITEA_INSTANCE_URL',
        'GITEA_URL',
        'CLOUDINIT_GITEA_INSTANCE_URL',
        'CLOUDINIT_GITEA_URL'
    )
}
if ([string]::IsNullOrWhiteSpace($RegistrationToken)) {
    $RegistrationToken = Resolve-FromEnvVarNames -DisplayName 'RegistrationToken' -Names @(
        'GITEA_RUNNER_REGISTRATION_TOKEN',
        'GITEA_RUNNER_TOKEN',
        'GITEA_REGISTRATION_TOKEN',
        'CLOUDINIT_GITEA_RUNNER_REGISTRATION_TOKEN',
        'CLOUDINIT_GITEA_RUNNER_TOKEN',
        'CLOUDINIT_GITEA_REGISTRATION_TOKEN'
    )
}

if ([string]::IsNullOrWhiteSpace($InstanceUrl))      { throw 'InstanceUrl not provided and no matching env var found.' }
if ([string]::IsNullOrWhiteSpace($RegistrationToken)) { throw 'RegistrationToken not provided and no matching env var found.' }

$platform = Get-PlatformDescriptor
if (-not $InstallRoot) { $InstallRoot = Get-DefaultInstallRoot }
if (-not $BinaryName)  { $BinaryName  = if ($IsWindows) { 'act_runner.exe' } else { 'act_runner' } }

$resolvedVersion = Resolve-ReleaseTag -Requested $Version
$assetName = "gitea-runner-$resolvedVersion-$($platform.Os)-$($platform.Arch)$($platform.Suffix)"
$downloadUri = "https://gitea.com/gitea/runner/releases/download/v$resolvedVersion/$assetName"

if (-not (Test-Path -LiteralPath $InstallRoot)) {
    Write-Stage "Creating install root $InstallRoot."
    New-Item -ItemType Directory -Path $InstallRoot -Force | Out-Null
}

$binaryPath = Join-Path $InstallRoot $BinaryName
$installedVersion = Get-InstalledBinaryVersion -BinaryPath $binaryPath

if ($installedVersion -eq $resolvedVersion -and -not $Force) {
    Write-Stage "act_runner $installedVersion already installed; skipping download."
} else {
    if ($installedVersion) { Write-Stage "Upgrading act_runner $installedVersion -> $resolvedVersion." }
    Save-BinaryViaWebClient -Uri $downloadUri -Destination $binaryPath
    if (-not $IsWindows) { & chmod 0755 $binaryPath | Out-Null }
}

Add-ToMachinePath -Directory $InstallRoot

Register-Runner -BinaryPath $binaryPath -WorkingDirectory $InstallRoot `
    -InstanceUrl $InstanceUrl -Token $RegistrationToken `
    -RunnerName $RunnerName -Labels $Labels -Force:$Force

if ($IsLinux)        { Install-SystemdUnit    -ServiceName $ServiceName -ServiceDisplayName $ServiceDisplayName -BinaryPath $binaryPath -WorkingDirectory $InstallRoot }
elseif ($IsMacOS)    { Install-LaunchdPlist   -ServiceName $ServiceName -ServiceDisplayName $ServiceDisplayName -BinaryPath $binaryPath -WorkingDirectory $InstallRoot }
elseif ($IsWindows)  { Install-WindowsService -ServiceName $ServiceName -ServiceDisplayName $ServiceDisplayName -BinaryPath $binaryPath -WorkingDirectory $InstallRoot }

Write-Stage "Gitea runner installation complete (version $resolvedVersion, service $ServiceName / '$ServiceDisplayName')."