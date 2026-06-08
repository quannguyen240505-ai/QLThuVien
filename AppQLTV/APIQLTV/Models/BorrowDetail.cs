using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace APIQLTV.Models;

[Table("borrowdetails")]
public class BorrowDetail
{
    [Key]
    public int BorrowDetailId { get; set; }

    public int BorrowTicketId { get; set; }
    public int BookId { get; set; }
    public int Quantity { get; set; } = 1;
    public DateTime BorrowDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? Status { get; set; }

    [JsonIgnore]
    public BorrowTicket? BorrowTicket { get; set; }
    [JsonIgnore]
    public Book? Book { get; set; }
}