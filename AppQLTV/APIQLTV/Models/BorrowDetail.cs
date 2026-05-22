namespace APIQLTV.Models
{
    public class BorrowDetail
    {
        public int Id { get; set; }

        public int BorrowTicketId { get; set; }
        public BorrowTicket? BorrowTicket { get; set; }

        public int BookId { get; set; }

        public int Quantity { get; set; } = 1;
        public string Status { get; set; } = "Borrowing";
    }
}