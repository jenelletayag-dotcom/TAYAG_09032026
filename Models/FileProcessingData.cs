namespace FileProcessingApi.Models
{
    public class FileProcessingData
    {
        public Guid Id { get; init; }

        public string FileName { get; init; } = string.Empty;


        public int RecordsCount { get; init; }

        public decimal Average { get; init; }

        public long ProcessingTimeMs { get; init; }

        public DateTimeOffset CreatedDate { get; init; }

    }
}
