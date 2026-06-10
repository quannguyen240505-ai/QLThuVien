namespace AppQLTV.Models.Books
{
    public class BookReview
    {
        public int ReviewId { get; set; }
        public int BookId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}