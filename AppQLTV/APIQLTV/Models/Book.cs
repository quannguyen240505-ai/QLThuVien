using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIQLTV.Models
{
    [Table("books")]
    public class Book
    {

        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? ISBN { get; set; }
        public string? Publisher { get; set; }
        public int PublishYear { get; set; }

        public string? Category { get; set; }  // Thể loại
        public string? Description { get; set; }

        public int TotalCopies { get; set; } = 1;      // Tổng số bản
        public int AvailableCopies { get; set; } = 1;  // Số bản còn lại

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}