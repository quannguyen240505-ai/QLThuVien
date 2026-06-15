using System.ComponentModel.DataAnnotations;

namespace APIQLTV.DTOs.Account
{
    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập Gmail.")]
        [EmailAddress(ErrorMessage = "Gmail không hợp lệ.")]
        public string Gmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngày sinh.")]
        public DateTime DateOfBirth { get; set; }
    }
}