namespace BASE.Models
{
    public class FileResultModel
    {
        public FileResultModel()
        {
            IsSuccess = true;
        }
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? FileID { get; set; }
        public string? FilePath { get; set; }
    }
}
