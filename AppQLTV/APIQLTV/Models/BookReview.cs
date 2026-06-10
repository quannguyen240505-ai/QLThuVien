using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIQLTV.Models;

[Table("bookreviews")]
public class BookReview
{
    [Key]
    public int ReviewId { get; set; }

    public int BookId { get; set; }

    public string UserId { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Comment { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsApproved { get; set; } = true; // true: hiển thị, false: ẩn

    [ForeignKey("BookId")]
    public virtual Book? Book { get; set; }
}