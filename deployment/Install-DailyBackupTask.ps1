<#
.SYNOPSIS
Scheduled backup task installer for RemediateVR.
.DESCRIPTION
Creates a Windows Scheduled Task that runs Backup-Databases.ps1 daily at 02:00.
.PARAMETER BackupDir
Backup directory.
.PARAMETER ServerInstance
SQL Server instance name.
#>

param(
    [string]$BackupDir = "C:\RemediateVR\Backups",
    [string]$ServerInstance = "(local)\SQLEXPRESS"
)

$ErrorActionPreference = "Stop"
if (-not (Test-Path $BackupDir)) { New-Item -ItemType Directory -Path $BackupDir | Out-Null }

$scriptPath = Join-Path $PSScriptRoot "Backup-Databases.ps1"
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$scriptPath`" -ServerInstance `"$ServerInstance`" -BackupDir `"$BackupDir`" -Compress"
$trigger = New-ScheduledTaskTrigger -Daily -At 2:00AM
$principal = New-ScheduledTaskPrincipal -UserId "NT AUTHORITY\SYSTEM" -LogonType ServiceAccount -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable

Register-ScheduledTask `
    -TaskName "RemediateVR-DailyBackup" `
    -Description "Daily compressed backup of RemediateVR databases" `
    -Action $action `
    -Trigger $trigger `
    -Principal $principal `
    -Settings $settings | Out-Null

Write-Host "Scheduled task 'RemediateVR-DailyBackup' created (daily at 02:00)." -ForegroundColor Green
