using System.ComponentModel.DataAnnotations;

namespace APIQLTV.DTOs.Admin
{
    public class AdminResetUserPasswordRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
