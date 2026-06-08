using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIQLTV.Models;

[Table("borrowtickets")] // tên bảng trong database (chữ thường)
public class BorrowTicket
{
    [Key]
    public int BorrowTicketId { get; set; }

    public int ReaderId { get; set; }

    public DateTime BorrowDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; }

    public string? Note { get; set; }

    public int OverdueDays { get; set; }

    public int FineAmount { get; set; }

    // Navigation properties
    [ForeignKey("ReaderId")]
    public virtual Reader? Reader { get; set; }

    public virtual ICollection<BorrowDetail>? BorrowDetails { get; set; }
}