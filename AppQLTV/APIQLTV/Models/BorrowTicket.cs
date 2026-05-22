namespace APIQLTV.Models
{
    public class BorrowTicket
    {
        public int Id { get; set; }

        public int ReaderId { get; set; }
        public Reader? Reader { get; set; }

        public DateTime BorrowDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public string Status { get; set; } = "Borrowing";
        public string? Note { get; set; }

        public ICollection<BorrowDetail>? BorrowDetails { get; set; }
    }
}