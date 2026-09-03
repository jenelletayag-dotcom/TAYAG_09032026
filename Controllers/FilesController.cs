using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using FileProcessingApi.Models;
using FileProcessingApi.Services.Interfaces;

namespace FileProcessingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly ILogService _logService;
        private readonly IFileService _fileService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(ILogService logService, IFileService fileService, ILogger<FilesController> logger)
        {
            _logService = logService;
            _fileService = fileService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded or file is empty." });
            }

            var fileName = file.FileName ?? string.Empty;
            var extension = Path.GetExtension(fileName);
            var contentType = file.ContentType ?? string.Empty;
            var isCsvExtension = string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase);
            var isCsvContentType = contentType.IndexOf("csv", StringComparison.OrdinalIgnoreCase) >= 0;

            if (!isCsvExtension && !isCsvContentType)
            {
                return BadRequest(new { message = "Only CSV files are supported." });
            }

            try
            {
                var response = await _fileService.ProcessFileAsync(file, cancellationToken);
                return Ok(response);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, new { message = "Request was cancelled." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request while processing file");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file {FileName}", file?.FileName);
                return StatusCode(500, new { message = "An error occurred while processing the file." });
            }
        }
    }
}