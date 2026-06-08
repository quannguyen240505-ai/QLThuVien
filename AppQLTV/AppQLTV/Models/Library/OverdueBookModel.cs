namespace AppQLTV.Models.Library
{
    public class OverdueBookModel
    {
        public int BorrowTicketId { get; set; }
        public string ReaderName { get; set; } = "";
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public int OverdueDays { get; set; }
        public int FineAmount { get; set; }
        public List<OverdueBookDetailModel> Details { get; set; } = new();
    }

    public class OverdueBookDetailModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = "";
        public int Quantity { get; set; }
    }
}