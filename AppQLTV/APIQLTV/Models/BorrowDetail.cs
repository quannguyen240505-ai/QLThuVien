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

    public string Status { get; set; } = "Borrowing";

    [JsonIgnore]
    public BorrowTicket? BorrowTicket { get; set; }
    [JsonIgnore]
    public Book? Book { get; set; }
}