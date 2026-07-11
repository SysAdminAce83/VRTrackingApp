using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VRTrackingApp.API.Services;

namespace VRTrackingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IFileUploadService fileUploadService, ILogger<UploadController> logger)
        {
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<FileProcessingResult>> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new FileProcessingResult 
                { 
                    Success = false, 
                    Message = "No file provided" 
                });
            }

            var result = await _fileUploadService.ProcessUploadedFileAsync(file);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }
}