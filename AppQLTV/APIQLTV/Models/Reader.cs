using System.ComponentModel.DataAnnotations;

namespace APIQLTV.Models
{
    public class Reader
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Active";

        public ICollection<BorrowTicket>? BorrowTickets { get; set; }
    }
}