using FileProcessingApi.Models;

namespace FileProcessingApi.Services.Interfaces
{
    public interface IFileService
    {
        Task<Response> ProcessFileAsync(
            IFormFile file,
            CancellationToken cancellationToken);
    }
}
