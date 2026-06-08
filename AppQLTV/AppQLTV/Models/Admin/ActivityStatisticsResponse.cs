namespace AppQLTV.Models.Admin
{
    public class ActivityStatisticsResponse
    {
        public int TotalUsers { get; set; }

        public int TotalMembers { get; set; }

        public int TotalLibrarians { get; set; }

        public int TotalAdmins { get; set; }

        public int TotalActiveUsers { get; set; }

        public int TotalLockedUsers { get; set; }

        public int TotalLocalAccounts { get; set; }

        public int TotalGoogleAccounts { get; set; }

        public string LibraryName { get; set; } = string.Empty;

        public bool AllowBorrowRequest { get; set; }

        public int MaxBorrowBooks { get; set; }

        public int MaxBorrowDays { get; set; }

        public decimal OverdueFinePerDay { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}