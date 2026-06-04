namespace AppQLTV.Models.Library
{
    public class ReaderModel
    {
        public int ReaderId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Address { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } = "Active";
    }
}