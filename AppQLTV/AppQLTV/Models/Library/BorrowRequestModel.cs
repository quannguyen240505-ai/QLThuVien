namespace AppQLTV.Models.Library
{
    public class BorrowRequestModel
    {
        public int ReaderId { get; set; }

        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);

        public string? Note { get; set; }

        public List<BorrowBookItemModel> Books { get; set; } = new();
    }
}