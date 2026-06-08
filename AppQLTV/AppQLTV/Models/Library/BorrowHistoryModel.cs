namespace AppQLTV.Models.Library
{
    public class BorrowHistoryModel
    {
        public int BorrowTicketId { get; set; }
        public int ReaderId { get; set; }
        public string ReaderName { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }

        public List<BorrowDetailHistoryModel> Details { get; set; } = new();
    }

    public class BorrowDetailHistoryModel
    {
        public int BorrowDetailId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}