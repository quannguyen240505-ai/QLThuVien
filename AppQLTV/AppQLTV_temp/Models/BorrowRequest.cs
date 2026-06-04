namespace AppQLTV.Models
{
    public class BorrowRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int BookId { get; set; }
        public string? BookTitle { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = "";
        public DateTime? ApproveDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Notes { get; set; }
    }
}