namespace APIQLTV.DTOs.Account
{
    public class AccountProfileResponse
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Gmail { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public string AuthProvider { get; set; } = "Local";

        public DateTime CreatedAt { get; set; }
    }
}