using System.ComponentModel.DataAnnotations;

namespace APIQLTV.DTOs.Borrow
{
    public class BorrowRequestCreateDTO
    {
        [Required]
        public int BookId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}