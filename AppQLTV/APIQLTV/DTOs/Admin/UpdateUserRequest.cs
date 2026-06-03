using System.ComponentModel.DataAnnotations;

namespace APIQLTV.DTOs.Admin
{
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Gmail.")]
        [EmailAddress(ErrorMessage = "Gmail không hợp lệ.")]
        public string Gmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngày sinh.")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Role { get; set; } = "Member";

        public bool IsActive { get; set; } = true;
    }
}
