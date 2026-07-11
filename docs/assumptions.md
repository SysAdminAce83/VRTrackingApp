# Assumptions About Nessus CSV and PDF Input

## Overview
This document outlines the assumptions made about the structure and content of Nessus scan outputs (CSV and PDF formats) that the vulnerability tracking system is designed to process.

## General Assumptions
1. Nessus scanner version 6.x or later is used for scanning
2. Scans are performed with credentialed and/or non-credentialed scans as appropriate
3. Plugins are up-to-date at the time of scanning
4. Standard Nessus plugins are used (no custom plugins unless specified)
5. Scan outputs are generated in the standard Nessus formats

## Nessus CSV Format Assumptions
The system assumes Nessus CSV exports follow the standard format with these columns:

### Required Columns
- **Plugin ID**: Numeric identifier of the Nessus plugin
- **Plugin Name**: Name of the vulnerability check
- **CVE**: Common Vulnerabilities and Exposures identifier (may be empty)
- **CVSS Base Score**: Numerical CVSS score (0.0-10.0)
- **Risk**: Risk level (Critical, High, Medium, Low, Info)
- **Host**: Target host IP address or hostname
- **Protocol**: Protocol (tcp, udp, etc.)
- **Port**: Port number
- **Name**: Service name
- **Synopsis**: Brief description of the vulnerability
- **Description**: Detailed description of the vulnerability
- **Solution**: Recommended remediation steps
- **See Also**: References to additional information
- **Plugin Output**: Raw output from the Nessus plugin
- **Risk Factor**: Additional risk information from Nessus

### Optional Columns (may be present but not required)
- CVSS Temporal Score
- CVSS Environmental Score
- Exploit Availability
- Patch Publication Date
- Plugin Publication Date
- Plugin Modification Date

### Data Format Assumptions
1. CSV files use commas as delimiters with proper quoting for fields containing commas
2. Character encoding is UTF-8 (with fallback to ASCII/ISO-8859-1)
3. Dates are in ISO 8601 format or similar parseable format
4. Numeric fields contain valid numbers or are empty
5. IP addresses are in valid IPv4 or IPv6 format

## Nessus PDF Format Assumptions
The system assumes Nessus PDF reports follow the standard report structure:

### Report Structure
1. **Report Header**: Contains scan metadata (scan name, date, etc.)
2. **Executive Summary**: High-level statistics and risk ratings
3. **Vulnerability Details**: Detailed findings for each vulnerability
4. **Host Details**: Information per scanned host
5. **Appendices**: Additional information, references, etc.

### Data Extraction Assumptions
1. Vulnerability information is presented in consistent sections/tables
2. Each vulnerability entry contains:
   - Plugin ID and name
   - Affected host(s)
   - Port/protocol information
   - Risk level
   - Description
   - Solution
   - References (CVE, etc.)
3. Text extraction from PDF will yield readable text content
4. Tables in PDF maintain structural integrity for data extraction

## Data Quality Assumptions
1. **Uniqueness**: Plugin ID + Host + Port + Protocol combination uniquely identifies a vulnerability instance
2. **Consistency**: Same plugin ID always refers to the same vulnerability check across scans
3. **Completeness**: Required fields for vulnerability identification are present
4. **Validity**: CVSS scores are within valid range (0.0-10.0)
5. **Consistency**: Risk levels map to standard Nessus categories

## Handling Variations
### Missing Data
- Missing CVSS scores: Treated as null/unknown
- Missing CVE values: Left blank but vulnerability still processed
- Missing hostnames: IP address used for identification
- Missing port information: Defaulted based on service or marked as unknown

### Duplicate Handling
- Exact duplicates (same Plugin ID, Host, Port, Protocol) within same scan are consolidated
- Vulnerabilities appearing across multiple scans are tracked as separate instances with timestamps

### Severity Mapping
Nessus risk levels map to system severity levels as follows:
- Critical → Critical
- High → High
- Medium → Medium
- Low → Low
- Info → Info

## Limitations and Known Issues
1. **PDF Parsing Complexity**: PDF structure varies significantly between report types and versions
2. **Custom Plugins**: Non-standard Nessus plugins may not follow expected field patterns
3. **Compliance Checks**: Compliance scan results may have different structure
4. **Localized Reports**: Non-English reports may require language-specific parsing
5. **Large Files**: Very large CSV/PDF files may require special handling for performance

## Validation Approach
1. **Format Validation**: Verify file is readable CSV or PDF
2. **Content Validation**: Check for presence of key columns/fields
3. **Data Validation**: Validate data types and ranges where applicable
4. **Error Reporting**: Provide clear messages for unsupported formats or corrupted files

## Future Considerations
1. Support for Nessus .nessus file format (native database)
2. Integration with Nessus Manager/API for direct retrieval
3. Support for other vulnerability scanners (Qualys, Rapid7, etc.)
4. Enhanced PDF parsing with OCR for scanned documents
5. Machine learning-based field extraction for variant formats