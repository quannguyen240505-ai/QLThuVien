using System.ComponentModel.DataAnnotations;

namespace APIQLTV.DTOs.Books
{
    public class BookCreateUpdateDTO
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;

        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [StringLength(100)]
        public string Publisher { get; set; } = string.Empty;

        public int PublishYear { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? ImageUrl { get; set; }
    }
}