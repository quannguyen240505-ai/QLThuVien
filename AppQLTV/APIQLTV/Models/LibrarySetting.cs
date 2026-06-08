namespace APIQLTV.Models
{
    public class LibrarySetting
    {
        public int Id { get; set; }

        public string LibraryName { get; set; } = "QNU Library";

        public string Email { get; set; } = "library@qnu.edu.vn";

        public string Phone { get; set; } = "";

        public string Address { get; set; } = "";

        public string OpeningHours { get; set; } = "07:30 - 20:30";

        public int MaxBorrowBooks { get; set; } = 5;

        public int MaxBorrowDays { get; set; } = 14;

        public decimal OverdueFinePerDay { get; set; } = 5000;

        public bool AllowBorrowRequest { get; set; } = true;

        public string LibraryRules { get; set; } = "";

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
