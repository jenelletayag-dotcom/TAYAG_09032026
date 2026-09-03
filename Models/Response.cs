namespace FileProcessingApi.Models
{
    public class Response
    {
        public string FileName { get; init; } = string.Empty;

        public int RecordsCount { get; init; }

        public decimal Average { get; init; }

        public long ProcessingTimeMs { get; init; }
    }
}
