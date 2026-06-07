namespace AppQLTV.Models.Library
{
    public class BookModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Author { get; set; }

        public string? ISBN { get; set; }

        public string? Publisher { get; set; }

        public int PublishYear { get; set; }

        public string? Category { get; set; }

        public int TotalCopies { get; set; }

        public int AvailableCopies { get; set; }

        public bool IsActive { get; set; }
    }
}