using FileProcessingApi.Models;

namespace FileProcessingApi.Services.Interfaces
{
    public interface ILogService
    {
        void LogProcessedFile(FileProcessingData data);
        IEnumerable<FileProcessingData> GetProcessedFiles();
    }
}