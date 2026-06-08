namespace APIQLTV.DTOs.Admin
{
    public class AdminStatisticsResponse
    {
        public int TotalUsers { get; set; }

        public int TotalMembers { get; set; }

        public int TotalLibrarians { get; set; }

        public int TotalAdmins { get; set; }

        public int TotalActiveUsers { get; set; }

        public int TotalLockedUsers { get; set; }

        public int TotalLocalAccounts { get; set; }

        public int TotalGoogleAccounts { get; set; }
    }
}
