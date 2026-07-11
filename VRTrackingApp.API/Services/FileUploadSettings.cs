namespace VRTrackingApp.API.Services
{
    public class FileUploadSettings
    {
        public string UploadDirectory { get; set; } = "Uploads";
        public long MaxFileSize { get; set; } = 104857600; // 100 MB
        public string[] AllowedExtensions { get; set; } = { ".csv", ".pdf" };
    }
}