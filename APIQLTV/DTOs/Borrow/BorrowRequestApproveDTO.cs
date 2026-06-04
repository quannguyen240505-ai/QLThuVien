using System.ComponentModel.DataAnnotations;

namespace APIQLTV.DTOs.Borrow
{
    public class BorrowRequestApproveDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public bool IsApproved { get; set; } // true: duyệt, false: từ chối

        [MaxLength(500)]
        public string? RejectReason { get; set; } // lý do nếu từ chối
    }
}