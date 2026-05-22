namespace AppQLTV.Models.Library
{
    public class BorrowHistoryModel
    {
        public int Id { get; set; }
        public string ReaderName { get; set; } = "";
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "";
        public string? Note { get; set; }
    }
}