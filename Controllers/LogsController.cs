using Microsoft.AspNetCore.Mvc;
using FileProcessingApi.Services.Interfaces;

namespace FileProcessingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogsController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public IActionResult GetProcessedFiles()
        {
            var logs = _logService.GetProcessedFiles();
            return Ok(logs);
        }
    }
}