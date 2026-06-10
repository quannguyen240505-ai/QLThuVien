using System;

namespace AppQLTV.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? ISBN { get; set; }
        public string? Publisher { get; set; }
        public int PublishYear { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CoverImageUrl { get; set; }
    }
}