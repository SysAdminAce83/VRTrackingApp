using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using VRTrackingApp.API.Services;

namespace VRTrackingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(IFileUploadService fileUploadService, ILogger<FilesController> logger)
        {
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<ActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file provided");

            try
            {
                var result = await _fileUploadService.ProcessUploadedFileAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file upload");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}