using System;
using System.IO;
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

            // Add checking for csv
            var fileName = file.FileName ?? string.Empty;
            var extension = Path.GetExtension(fileName);
            var contentType = file.ContentType ?? string.Empty;

            if (!string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase)
                && !contentType.Contains("csv", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only CSV files are supported.");
            }

            var startTime = DateTimeOffset.UtcNow;
            var values = new List<decimal>();
            int recordsCount = 0;

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            var header = await reader.ReadLineAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(header) || (!header.Contains(',') && !header.TrimStart().StartsWith("\uFEFF") && !header.TrimStart().StartsWith("{" ) && !header.TrimStart().StartsWith("[")))
            {
                throw new ArgumentException("Only CSV files are supported.");
            }

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