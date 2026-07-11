namespace VRTrackingApp.API.Services.DTOs
{
    public class FileProcessingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? ScanId { get; set; }
        public string? FilePath { get; set; }
    }
}