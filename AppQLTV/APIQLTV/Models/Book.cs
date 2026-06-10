using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace APIQLTV.Models;

[Table("books")]
public class Book
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Author { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ISBN { get; set; }

    [MaxLength(150)]
    public string? Publisher { get; set; }

    [Column("PublishYear")]
    public int? PublishYear { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public string? Description { get; set; }

    [Column("TotalCopies")]
    public int TotalCopies { get; set; } = 1;

    [Column("AvailableCopies")]
    public int AvailableCopies { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public ICollection<BorrowDetail>? BorrowDetails { get; set; }

    public string? CoverImageUrl { get; set; }
}