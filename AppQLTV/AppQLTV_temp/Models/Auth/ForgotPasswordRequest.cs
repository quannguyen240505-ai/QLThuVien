using System.ComponentModel.DataAnnotations;

namespace AppQLTV.Models.Auth
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập Gmail.")]
        [EmailAddress(ErrorMessage = "Gmail không hợp lệ.")]
        public string Gmail { get; set; } = string.Empty;
    }
}
