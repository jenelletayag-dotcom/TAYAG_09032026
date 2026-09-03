using System.Collections.Concurrent;
using FileProcessingApi.Models;
using FileProcessingApi.Services.Interfaces;

namespace FileProcessingApi.Services
{
    public class LogService : ILogService
    {
        private readonly ConcurrentBag<FileProcessingData> _processedFiles = new();

        public void LogProcessedFile(FileProcessingData data)
        {
            _processedFiles.Add(data);
        }

        public IEnumerable<FileProcessingData> GetProcessedFiles()
        {
            return _processedFiles.OrderByDescending(d => d.CreatedDate);
        }
    }
}