namespace AppQLTV.Models.Auth
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Gmail { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string Role { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}
