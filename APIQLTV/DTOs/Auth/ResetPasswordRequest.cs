using System.ComponentModel.DataAnnotations;

namespace APIQLTV.DTOs.Auth
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập Gmail.")]
        [EmailAddress(ErrorMessage = "Gmail không hợp lệ.")]
        public string Gmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã PIN.")]
        public string Pin { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới.")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
