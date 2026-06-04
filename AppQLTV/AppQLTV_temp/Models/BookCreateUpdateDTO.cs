namespace AppQLTV.Models
{
    public class BookCreateUpdateDTO
    {
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Category { get; set; } = "";
        public string Publisher { get; set; } = "";
        public int PublishYear { get; set; }
        public int Quantity { get; set; }
    }
}