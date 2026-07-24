<#
.SYNOPSIS
Restores a RemediateVR database from a backup manifest.
.DESCRIPTION
Restores the latest .bak or .bak.gz backup for RemediateVR from the local backup directory and sets the database online.
.PARAMETER ServerInstance
SQL Server instance name.
.PARAMETER BackupDir
Directory containing backup files.
.PARAMETER DatabaseName
Logical database name to restore (default: RemediateVR).
.EXAMPLE
.\Restore-Databases.ps1 -ServerInstance "(local)\SQLEXPRESS" -DatabaseName "RemediateVR"
#>

param(
    [string]$ServerInstance = "(local)\SQLEXPRESS",
    [string]$BackupDir = "C:\RemediateVR\Backups",
    [string]$DatabaseName = "RemediateVR"
)

$ErrorActionPreference = "Stop"
if (-not (Test-Path $BackupDir)) { Write-Error "Backup directory not found: $BackupDir" }

$candidates = Get-ChildItem $BackupDir -Filter "${DatabaseName}_*.bak*" | Sort-Object LastWriteTime -Descending
if (-not $candidates) {
    $candidates = Get-ChildItem $BackupDir -Filter "${DatabaseName}_*.bak.gz" | Sort-Object LastWriteTime -Descending
    if (-not $candidates) {
        Write-Error "No backups found for $DatabaseName in $BackupDir"
    }
}

$artifact = $candidates[0]
Write-Host "Restoring $($artifact.Name)..." -ForegroundColor Cyan

$bakPath = $artifact.FullName
if ($artifact.Extension -eq ".gz") {
    $bakPath = [System.IO.Path]::ChangeExtension($artifact.FullName, ".bak")
    Write-Host "Decompressing..."
    $src = [System.IO.File]::OpenRead($artifact.FullName)
    $dst = [System.IO.File]::Create($bakPath)
    $stream = New-Object System.IO.Compression.GZipStream($dst, [System.IO.Compression.CompressionLevel]::Optimal)
    $src.CopyTo($stream)
    $stream.Dispose(); $src.Dispose(); $dst.Dispose()
}

$sql = @"
IF EXISTS (SELECT * FROM sys.databases WHERE name = N'$DatabaseName')
BEGIN
    ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    RESTORE DATABASE [$DatabaseName] FROM DISK = N'$bakPath' WITH REPLACE, RECOVERY;
    ALTER DATABASE [$DatabaseName] SET MULTI_USER;
END
ELSE
BEGIN
    RESTORE DATABASE [$DatabaseName] FROM DISK = N'$bakPath' WITH RECOVERY;
END
"@
Invoke-Sqlcmd -ServerInstance $ServerInstance -Query $sql -ErrorAction Stop
Write-Host "Database [$DatabaseName] restored successfully." -ForegroundColor Green

if ($artifact.Extension -eq ".gz") { Remove-Item $bakPath }
