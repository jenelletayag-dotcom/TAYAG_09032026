using Microsoft.AspNetCore.Mvc;
using FileProcessingApi.Models;
using FileProcessingApi.Services.Interfaces;
using System.Diagnostics;

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
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded or file is empty." });
            }

            var stopwatch = Stopwatch.StartNew();
            int recordCount = 0;
            decimal average = 0;

            try
            {
                using var streamReader = new StreamReader(file.OpenReadStream());
                var lines = await streamReader.ReadToEndAsync();
                var rows = lines.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                var dataRows = rows.Skip(1).ToList();
                recordCount = dataRows.Count;

                if (recordCount > 0 && rows.Length > 0)
                {
                    // Dynamically find the column index for "Score" based on the header row
                    var headerColumns = rows[0].Split(',');
                    int scoreIndex = Array.FindIndex(headerColumns, h => h.Trim().Equals("Score", StringComparison.OrdinalIgnoreCase));

                    if (scoreIndex != -1)
                    {
                        decimal sum = 0;
                        int validCount = 0;
                        foreach (var row in dataRows)
                        {
                            var columns = row.Split(',');
                            if (columns.Length > scoreIndex && decimal.TryParse(columns[scoreIndex], out var val))
                            {
                                sum += val;
                                validCount++;
                            }
                        }
                        average = validCount > 0 ? sum / validCount : 0;
                    }
                }

                stopwatch.Stop();

                var response = new Response
                {
                    FileName = file.FileName,
                    RecordsCount = recordCount,
                    Average = average,
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds
                };

                var processingData = new FileProcessingData
                {
                    FileName = response.FileName,
                    RecordsCount = response.RecordsCount,
                    Average = response.Average,
                    ProcessingTimeMs = response.ProcessingTimeMs,
                    CreatedDate = DateTimeOffset.UtcNow
                };

                _logService.LogProcessedFile(processingData);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file {FileName}", file.FileName);
                return StatusCode(500, new { message = "An error occurred while processing the file." });
            }
        }
    }
}