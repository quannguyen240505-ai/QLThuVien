namespace AppQLTV.Models.Library
{
    public class ReaderModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string Status { get; set; } = "Active";
    }
}