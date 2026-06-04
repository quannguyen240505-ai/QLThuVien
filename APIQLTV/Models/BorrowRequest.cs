using APIQLTV.Models.Books;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIQLTV.Models
{
    public class BorrowRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime? ApproveDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime? DueDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }

        [ForeignKey("BookId")]
        public virtual Book? Book { get; set; }
    }
}