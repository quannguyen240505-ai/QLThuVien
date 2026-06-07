namespace AppQLTV.Models.Library
{
    public class BorrowRequest
    {
        public int ReaderId { get; set; }
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);
        public string? Note { get; set; }
        public List<BorrowBookItem> Books { get; set; } = new();
    }

    public class BorrowBookItem
    {
        public int BookId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}