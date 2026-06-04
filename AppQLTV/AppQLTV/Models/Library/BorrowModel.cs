namespace AppQLTV.Models.Library
{
    public class BorrowModel
    {
        public int ReaderId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(14);
    }
}