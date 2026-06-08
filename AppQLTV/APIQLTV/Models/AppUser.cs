namespace APIQLTV.Models
{
    public class AppUser
    {
        public int Id { get; set; }


        public string Username { get; set; } = string.Empty;

        public string Gmail { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Member";
        public string AuthProvider { get; set; } = "Local";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? ResetPasswordPin { get; set; }

        public DateTime? ResetPasswordPinExpires { get; set; }

        public bool IsResetPasswordPinUsed { get; set; } = false;
    }
}
