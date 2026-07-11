using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VRTrackingApp.Infrastructure.DependencyInjection;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.Models;

namespace VRTrackingApp.API.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IConfiguration _configuration;
        private readonly IScanRepository _scanRepository;
        private readonly ILogger<FileUploadService> _logger;
        private readonly FileUploadSettings _settings;

        public FileUploadService(
            IConfiguration configuration,
            IScanRepository scanRepository,
            ILogger<FileUploadService> logger)
        {
            _configuration = configuration;
            _scanRepository = scanRepository;
            _logger = logger;
            _settings = configuration.GetSection("FileUploadSettings").Get<FileUploadSettings>()
                       ?? new FileUploadSettings();
        }

        public async Task<FileProcessingResult> ProcessUploadedFileAsync(IFormFile file)
        {
            var result = new FileProcessingResult();

            try
            {
                // Validate file
                if (!IsValidFile(file))
                {
                    result.Success = false;
                    result.Message = $"Invalid file. Allowed extensions: {string.Join(", ", _settings.AllowedExtensions)}. Maximum size: {_settings.MaxFileSize} bytes.";
                    return result;
                }

                // Create uploads directory if it doesn't exist
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), _settings.UploadDirectory);
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Calculate file hash (SHA-256)
                var fileHash = await CalculateFileHashAsync(filePath);

                // Create scan record
                var scan = new Scan
                {
                    ScanName = $"Scan_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                    ScanDate = DateTime.UtcNow,
                    ScanType = Path.GetExtension(file.FileName).ToLower() == ".csv" ? "CSV Import" : "PDF Import",
                    FileName = file.FileName,
                    FileSize = file.Length,
                    FileHash = fileHash,
                    Status = "Processing",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdScan = await _scanRepository.AddAsync(scan);
                result.ScanId = createdScan.Id;
                result.FilePath = filePath;
                result.Success = true;
                result.Message = "File uploaded successfully. Processing will begin shortly.";

                // TODO: Trigger the parsing process (this would typically be done via a background job)
                // For now, we'll just return success and the parsing can be triggered separately

                _logger.LogInformation("File uploaded successfully. File: {FileName}, ScanId: {ScanId}", file.FileName, createdScan.Id);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error processing file: {ex.Message}";
                _logger.LogError(ex, "Error processing uploaded file");
            }

            return result;
        }

        private bool IsValidFile(IFormFile file)
        {
            // Check file size
            if (file.Length > _settings.MaxFileSize)
            {
                return false;
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _settings.AllowedExtensions.Contains(extension);
        }

        private async Task<string> CalculateFileHashAsync(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}