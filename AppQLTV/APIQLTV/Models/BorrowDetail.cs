using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIQLTV.Models;

[Table("borrowdetails")]
public class BorrowDetail
{
    [Key]
    public int BorrowDetailId { get; set; }

    public int BorrowTicketId { get; set; }

<<<<<<< Updated upstream
        public int BookId { get; set; }
        public Book? Book { get; set; }

        public int Quantity { get; set; }

        public string? Status { get; set; }
    }
=======
    public int BookId { get; set; }

    public int Quantity { get; set; } = 1;

    public string Status { get; set; } = "Borrowing";

    [ForeignKey("BorrowTicketId")]
    public BorrowTicket? BorrowTicket { get; set; }

    [ForeignKey("BookId")]
    public Book? Book { get; set; }
>>>>>>> Stashed changes
}