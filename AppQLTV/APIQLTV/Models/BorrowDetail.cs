namespace APIQLTV.Models
{
    public class BorrowDetail
    {
        public int BorrowDetailId { get; set; }

        public int BorrowTicketId { get; set; }
        public BorrowTicket? BorrowTicket { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

        public int Quantity { get; set; }

        public string? Status { get; set; }
    }
}