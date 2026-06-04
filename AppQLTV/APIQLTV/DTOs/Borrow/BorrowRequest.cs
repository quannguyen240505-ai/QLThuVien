namespace APIQLTV.DTOs.Borrow
{
    public class BorrowRequest
    {
        public int ReaderId { get; set; }
        public DateTime DueDate { get; set; }
        public string? Note { get; set; }
        public List<BorrowBookItem> Books { get; set; } = new();
    }
}