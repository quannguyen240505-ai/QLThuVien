namespace AppQLTV.Models.Admin
{
    public class LibrarySettingResponse
    {
        public int Id { get; set; }

        public string LibraryName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string OpeningHours { get; set; } = string.Empty;

        public int MaxBorrowBooks { get; set; }

        public int MaxBorrowDays { get; set; }

        public decimal OverdueFinePerDay { get; set; }

        public bool AllowBorrowRequest { get; set; }

        public string LibraryRules { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; }
    }
}
