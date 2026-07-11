# Vulnerability Tracking System Database Schema

## Overview
This document describes the normalized database schema for the vulnerability tracking system. The schema is designed to efficiently store vulnerability data from Nessus scans while supporting querying, filtering, and reporting.

## Database Schema Diagram (Textual Representation)

```
[Scans] 1 ------ * [Assets]
   |                   |
   |                   *------- [AssetVulnerabilities] *------- [Vulnerabilities]
   |                                   |
   *                                   *
[ScanAssets]                          [References]
```

## Tables

### 1. Scans
Stores information about each vulnerability scan.

| Column Name | Data Type | Constraints | Description |
|-------------|-----------|-------------|-------------|
| Id | INT | PK, IDENTITY | Primary key |
| ScanName | NVARCHAR(255) | NOT NULL | Name/description of the scan |
| ScanDate | DATETIME2 | NOT NULL | Date/time when scan was performed |
|---| NVARCHAR(50) | NOT NULL | Type of scanType | NVARCHAR(50) | NOT NULL | Type of scan (e.g., 'Nessus Scan') |
| FileName | NVARCHAR(255) | NOT NULL | Original filename uploaded |
| FileSize | BIGINT | NULL | Size of uploaded file in bytes |
| FileHash | VARCHAR(64) | NULL | SHA-256 hash of uploaded file |
| Status | NVARCHAR(50) | NOT NULL DEFAULT 'Processing' | Status (Processing, Completed, Failed) |
| CreatedAt | DATETIME2 | NOT NULL DEFAULT GETDATE() | Audit timestamp |
| UpdatedAt | DATETIME2 | NULL | Audit timestamp |

### 2. Assets
Stores information about scanned assets (hosts, devices).

| Column Name | Data Type | Constraints | Description |
|-------------|-----------|-------------|-------------|
| Id | INT | PK, IDENTITY | Primary key |
| ScanId | INT | FK to Scans.Id | Reference to parent scan |
| HostName | NVARCHAR(255) | NULL | Hostname of the asset |
| IPAddress | VARCHAR(45) | NOT NULL | IP address (IPv4 or IPv6) |
| MACAddress | VARCHAR(17) | NULL | MAC address (if available) |
| NetBIOSName | NVARCHAR(255) | NULL | NetBIOS name |
| DNSName | NVARCHAR(255) | NULL | DNS name |
| OperatingSystem | NVARCHAR(255) | NULL | Detected operating system |
| OSVersion | NVARCHAR(100) | NULL | OS version details |
| CreatedAt | DATETIME2 | NOT NULL DEFAULT GETDATE() | Audit timestamp |
| UpdatedAt | DATETIME2 | NULL | Audit timestamp |

### 3. Vulnerabilities
Stores unique vulnerability definitions (plugin-level information).

| Column Name | Data Type | Constraints | Description |
|-------------|-----------|-------------|-------------|
| Id | INT | PK, IDENTITY | Primary key |
| PluginID | INT | NOT NULL | Nessus plugin ID |
| PluginName | NVARCHAR(255) | NOT NULL | Name of the vulnerability plugin |
| CVE | VARCHAR(20) | NULL | CVE identifier (if available) |
| CVSSBaseScore | FLOAT | NULL | CVSS base score |
| CVSSVector | NVARCHAR(100) | NULL | CVSS vector string |
| Severity | NVARCHAR(20) | NOT NULL | Severity level (Critical, High, Medium, Low, Info) |
| Description | NVARCHAR(MAX) | NULL | Detailed description |
| Solution | NVARCHAR(MAX) | NULL | Remediation solution |
| SeeAlso | NVARCHAR(MAX) | NULL | Related references |
| PluginPublicationDate | DATETIME2 | NULL | Date plugin was published |
| PluginModificationDate | DATETIME2 | NULL | Date plugin was last modified |
| ExploitAvailable | BIT | NULL | Whether exploit is available |
| PatchPublicationDate | DATETIME2 | NULL | Date patch was published |
| CreatedAt | DATETIME2 | NOT NULL DEFAULT GETDATE() | Audit timestamp |
| UpdatedAt | DATETIME2 | NULL | Audit timestamp |

### 4. AssetVulnerabilities
Junction table linking assets to their vulnerabilities (instance-level data).

| Column Name | Data Type | Constraints | Description |
|-------------|-----------|-------------|-------------|
| Id | INT | PK, IDENTITY | Primary key |
| AssetId | INT | FK to Assets.Id | Reference to asset |
| VulnerabilityId | INT | FK to Vulnerabilities.Id | Reference to vulnerability |
| Port | INT | NULL | Port number (if applicable) |
| Protocol | VARCHAR(10) | NULL | Protocol (tcp, udp, etc.) |
| ServiceName | NVARCHAR(100) | NULL | Service name running on port |
| PluginOutput | NVARCHAR(MAX) | NULL | Raw plugin output from Nessus |
| RiskFactor | NVARCHAR(50) | NULL | Risk factor from Nessus |
| FirstFound | DATETIME2 | NULL | First time vulnerability was detected |
| LastFound | DATETIME2 | NULL | Last time vulnerability was detected |
| Status | NVARCHAR(20) | NOT NULL DEFAULT 'Active' | Status (Active, Fixed, False Positive, etc.) |
| CreatedAt | DATETIME2 | NOT NULL DEFAULT GETDATE() | Audit timestamp |
| UpdatedAt | DATETIME2 | NULL | Audit timestamp |

### 5. References
Stores external references for vulnerabilities (CVE URLs, bugtraq, etc.).

| Column Name | Data Type | Constraints | Description |
|-------------|-----------|-------------|-------------|
| Id | INT | PK, IDENTITY | Primary key |
| VulnerabilityId | INT | FK to Vulnerabilities.Id | Reference to vulnerability |
| ReferenceType | NVARCHAR(50) | NOT NULL | Type (CVE, BID, URL, etc.) |
| ReferenceValue | NVARCHAR(500) | NOT NULL | Reference value (URL, ID, etc.) |
| URL | NVARCHAR(2048) | NULL | URL if reference is a web link |
| CreatedAt | DATETIME2 | NOT NULL DEFAULT GETDATE() | Audit timestamp |

## Indexes
- **Scans**: IX_Scans_ScanDate (ScanDate DESC)
- **Assets**: IX_Assets_ScanId (ScanId), IX_Assets_IPAddress (IPAddress)
- **Vulnerabilities**: IX_Vulnerabilities_PluginID (PluginID), IX_Vulnerabilities_CVE (CVE), IX_Vulnerabilities_Severity (Severity)
- **AssetVulnerabilities**: IX_AssetVulnerabilities_AssetId (AssetId), IX_AssetVulnerabilities_VulnerabilityId (VulnerabilityId), IX_AssetVulnerabilities_Status (Status)
- **References**: IX_References_VulnerabilityId (VulnerabilityId)

## Relationships
- One Scan → Many Assets (1:M)
- One Asset → Many AssetVulnerabilities (1:M)
- One Vulnerability → Many AssetVulnerabilities (1:M)
- One Vulnerability → Many References (1:M)
- Many Assets ↔ Many Vulnerabilities (through AssetVulnerabilities)

## Design Considerations
1. **Normalization**: Schema is normalized to 3NF to eliminate redundancy
2. **Extensibility**: New vulnerability attributes can be added to Vulnerabilities table
3. **Performance**: Strategic indexes on frequently queried columns
4. **Auditability**: CreatedAt/UpdatedAt timestamps on all tables
5. **Flexibility**: Nullable fields accommodate varying Nessus scan outputs
6. **Security**: No sensitive data stored in plaintext (file hashes only)

## Sample Data
*See sample_data.sql for example insert statements*

## Migration Strategy
The schema is designed to be compatible with Entity Framework Core migrations. Initial migration will create all tables, indexes, and relationships.

## Future Enhancements
1. Partitioning for large scan tables
2. Additional tables for scan comparison/trending
3. Tables for ticketing system integration
4. Supplemental tables for asset grouping/tagging