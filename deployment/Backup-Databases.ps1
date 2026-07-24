<#
.SYNOPSIS
Creates a compressed backup of the RemediateVR SQL Server databases.
.DESCRIPTION
Runs BACKUP DATABASE for RemediateVR and RemediateVR_Audit (if present), compresses the .bak files, and writes a restore manifest.
.PARAMETER ServerInstance
SQL Server instance name.
.PARAMETER BackupDir
Directory where backup files are stored. Creates folder if missing.
.PARAMETER Compress
Compress backup files using System.IO.Compression.GzipStream.
#>

param(
    [string]$ServerInstance = "(local)\SQLEXPRESS",
    [string]$BackupDir = "C:\RemediateVR\Backups",
    [switch]$Compress
)

$ErrorActionPreference = "Stop"
if (-not (Test-Path $BackupDir)) { New-Item -ItemType Directory -Path $BackupDir | Out-Null }

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$databases = @("RemediateVR")
$manifest = [System.Collections.Generic.List[string]]()
$manifest.Add("Backup run: $timestamp")
$manifest.Add("Server: $ServerInstance")

foreach ($db in $databases) {
    $bak = Join-Path $BackupDir ("${db}_${timestamp}.bak")
    Write-Host "Backing up [$db] -> $bak"
    $sql = "BACKUP DATABASE [$db] TO DISK = N'$bak' WITH FORMAT, NAME = N'$db-Full Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10"
    Invoke-Sqlcmd -ServerInstance $ServerInstance -Query $sql -ErrorAction Stop
    if ($Compress) {
        $gzip = "$bak.gz"
        Write-Host "Compressing $bak -> $gzip"
        $src = [System.IO.File]::OpenRead($bak)
        $dst = [System.IO.File]::Create($gzip)
        $stream = New-Object System.IO.Compression.GZipStream($dst, [System.IO.Compression.CompressionLevel]::Optimal)
        $src.CopyTo($stream)
        $stream.Dispose(); $src.Dispose(); $dst.Dispose()
        Remove-Item $bak
        $manifest.Add("  $db -> $gzip")
    } else {
        $manifest.Add("  $db -> $bak")
    }
}

$manifestPath = Join-Path $BackupDir ("BackupManifest_${timestamp}.txt")
$manifest | Out-File $manifestPath
Write-Host "Backup manifest written to $manifestPath" -ForegroundColor Green
