using System.ComponentModel.DataAnnotations;

namespace APIQLTV.DTOs.Admin
{
    public class UpdateLibrarySettingRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập tên thư viện.")]
        public string LibraryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email liên hệ.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string OpeningHours { get; set; } = string.Empty;

        [Range(1, 50, ErrorMessage = "Số sách tối đa phải từ 1 đến 50.")]
        public int MaxBorrowBooks { get; set; }

        [Range(1, 365, ErrorMessage = "Số ngày mượn tối đa phải từ 1 đến 365.")]
        public int MaxBorrowDays { get; set; }

        [Range(0, 1000000, ErrorMessage = "Phí phạt không hợp lệ.")]
        public decimal OverdueFinePerDay { get; set; }

        public bool AllowBorrowRequest { get; set; }

        public string LibraryRules { get; set; } = string.Empty;
    }
}
