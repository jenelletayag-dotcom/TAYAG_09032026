using FileProcessingApi.Models;
using FileProcessingApi.Services.Interfaces;

namespace FileProcessingApi.Services
{
    public class FileService : IFileService
    {
        private readonly ILogService _logService;

        public FileService(ILogService logService)
        {
            _logService = logService;
        }

        public async Task<Response> ProcessFileAsync(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            var startTime = DateTimeOffset.UtcNow;
            var values = new List<decimal>();
            int recordsCount = 0;

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            var header = await reader.ReadLineAsync(cancellationToken);

            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var line = await reader.ReadLineAsync(cancellationToken);
                if (line == null)
                    continue;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');

                if (parts.Length > 1 && decimal.TryParse(parts[1].Trim(), out var parsedValue))
                {
                    values.Add(parsedValue);
                }

                recordsCount++;
            }

            decimal average = values.Count > 0 ? values.Average() : 0;
            var processingTimeMs = (long)(DateTimeOffset.UtcNow - startTime).TotalMilliseconds;

            var response = new Response
            {
                FileName = file.FileName,
                RecordsCount = recordsCount,
                Average = average,
                ProcessingTimeMs = processingTimeMs
            };

            var processingData = new FileProcessingData
            {
                Id = Guid.NewGuid(),
                FileName = response.FileName,
                RecordsCount = response.RecordsCount,
                Average = response.Average,
                ProcessingTimeMs = response.ProcessingTimeMs,
                CreatedDate = DateTimeOffset.UtcNow
            };

            _logService.LogProcessedFile(processingData);

            return response;
        }
    }
}