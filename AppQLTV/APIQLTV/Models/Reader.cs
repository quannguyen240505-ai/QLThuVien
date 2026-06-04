using System.ComponentModel.DataAnnotations;

namespace APIQLTV.Models
{
    public class Reader
    {
        public int ReaderId { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string? Address { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Active";

        public ICollection<BorrowTicket>? BorrowTickets { get; set; }
        public DateTime? DateOfBirth { get; internal set; }
        public string? Gender { get; internal set; }
    }
}