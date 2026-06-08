using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIQLTV.Models
{
    [Table("borrowtickets")]
    public class BorrowTicket
    {
        [Key]
        public int BorrowTicketId { get; set; }

        public int ReaderId { get; set; }

        public Reader? Reader { get; set; }

        public DateTime BorrowDate { get; set; } = DateTime.Now;

        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public string? Status { get; set; }

        public string? Note { get; set; }
        public int FineAmount { get; set; }
        public int OverdueDays { get; set; }

        public ICollection<BorrowDetail> BorrowDetails { get; set; } = new List<BorrowDetail>();
    }
}