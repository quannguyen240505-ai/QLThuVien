namespace APIQLTV.Models
{
    public class Reader
    {
        public int ReaderId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? Status { get; set; }
    }
}