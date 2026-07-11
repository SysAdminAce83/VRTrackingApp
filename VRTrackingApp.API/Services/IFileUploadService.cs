using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VRTrackingApp.API.Services
{
    public interface IFileUploadService
    {
        Task<FileProcessingResult> ProcessUploadedFileAsync(IFormFile file);
    }
}