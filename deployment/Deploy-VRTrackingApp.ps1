<#
.SYNOPSIS
Deploys RemediateVR to IIS on Windows Server.
.DESCRIPTION
Publishes the .NET Web project, configures IIS application pool, and sets the site.
.PARAMETER ServerName
Target IIS server hostname (default: localhost).
.PARAMETER SiteName
IIS site name (default: RemediateVR).
.PARAMETER AppPoolName
IIS application pool name (default: RemediateVRPool).
.PARAMETER PublishDir
Path to pre-published web root. If omitted, the script publishes to a temp folder.
.PARAMETER SkipPublish
Set to true when the app is already published.
.EXAMPLE
.\Deploy-VRTrackingApp.ps1 -ServerName "WEB01" -SiteName "RemediateVR-Prod"
#>

param(
    [string]$ServerName = "localhost",
    [string]$SiteName = "RemediateVR",
    [string]$AppPoolName = "RemediateVRPool",
    [string]$PublishDir = "",
    [switch]$SkipPublish
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$WebProject = Join-Path $RepoRoot "VRTrackingApp\VRTrackingApp.Web\VRTrackingApp.Web.csproj"

if (-not (Test-Path $WebProject)) {
    Write-Error "Web project not found at $WebProject"
}

if (-not $SkipPublish -or [string]::IsNullOrWhiteSpace($PublishDir)) {
    $PublishDir = Join-Path $env:TEMP ("RemediateVR_Publish_" + [guid]::NewGuid().ToString("N"))
    Write-Host "Publishing to $PublishDir" -ForegroundColor Cyan
    dotnet publish $WebProject `
        --configuration Release `
        --output $PublishDir `
        --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Error "dotnet publish failed"
    }
}

Write-Host "Ensuring IIS is present on $ServerName..."
$iis = Get-WindowsFeature -Name Web-Server -ComputerName $ServerName -ErrorAction SilentlyContinue
if ($iis -and -not $iis.Installed) {
    Write-Host "Installing IIS..."
    Install-WindowsFeature -Name Web-Server,Web-Asp-Net45,Web-Server -IncludeAllSubFeature -IncludeManagementTools -ComputerName $ServerName | Out-Null
}

Write-Host "Creating/updating Application Pool '$AppPoolName' ($NoManagedCode)..."
Import-Module WebAdministration -ErrorAction SilentlyContinue | Out-Null
$appPoolPath = "IIS:\AppPools\$AppPoolName"
if (-not (Test-Path $appPoolPath)) {
    New-WebAppPool -Name $AppPoolName | Out-Null
}
Set-ItemProperty $appPoolPath -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty $appPoolPath -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
Set-ItemProperty $appPoolPath -Name "processModel.pingingEnabled" -Value $true
Set-ItemProperty $appPoolPath -Name "processModel.pingingInterval" -Value "00:01:00"
Set-ItemProperty $appPoolPath -Name "processModel.idleTimeout" -Value "00:20:00"
Set-ItemProperty $appPoolPath -Name "recycling.periodicRestart.time" -Value "00:00:00"

Write-Host "Creating/updating Site '$SiteName'..."
if (-not (Test-Path "IIS:\Sites\$SiteName")) {
    if (-not (Test-Path "IIS:\Sites\Default Web Site")) {
        New-Item "IIS:\Sites\$SiteName" -bindings @{protocol="http";bindingInformation="*:80:"} -physicalPath $PublishDir | Out-Null
    } else {
        New-Item "IIS:\Sites\$SiteName" -bindings @{protocol="http";bindingInformation="*:80:"} -physicalPath $PublishDir | Out-Null
    }
} else {
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name "physicalPath" -Value $PublishDir
}

Set-ItemProperty "IIS:\Sites\$SiteName" -Name "applicationPool" -Value $AppPoolName

Write-Host "Starting site '$SiteName'..."
Start-WebSite -Name $SiteName

Write-Host "Deployment complete. URL: http://$ServerName/" -ForegroundColor Green
Write-Host "Published folder: $PublishDir"
