using APIQLTV.DTOs.Borrow;
using APIQLTV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIQLTV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BorrowController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> BorrowBooks(BorrowRequest request)
        {
            var reader = await _context.Readers.FindAsync(request.ReaderId);

            if (reader == null)
                return NotFound("Không tìm thấy độc giả");

            if (request.Books == null || request.Books.Count == 0)
                return BadRequest("Chưa chọn sách để mượn");

            var ticket = new BorrowTicket
            {
                ReaderId = request.ReaderId,
                DueDate = request.DueDate,
                Note = request.Note,
                Status = "Borrowing"
            };

            _context.BorrowTickets.Add(ticket);
            await _context.SaveChangesAsync();

            foreach (var item in request.Books)
            {
                var detail = new BorrowDetail
                {
                    BorrowTicketId = ticket.Id,
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    Status = "Borrowing"
                };

                _context.BorrowDetails.Add(detail);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Mượn sách thành công",
                borrowTicketId = ticket.Id
            });
        }

        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnBooks(int id)
        {
            var ticket = await _context.BorrowTickets
                .Include(t => t.BorrowDetails)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return NotFound("Không tìm thấy phiếu mượn");

            if (ticket.Status == "Returned")
                return BadRequest("Phiếu này đã được trả trước đó");

            ticket.Status = "Returned";
            ticket.ReturnDate = DateTime.Now;

            if (ticket.BorrowDetails != null)
            {
                foreach (var detail in ticket.BorrowDetails)
                {
                    detail.Status = "Returned";
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Trả sách thành công" });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetBorrowHistory()
        {
            var history = await _context.BorrowTickets
                .Include(t => t.Reader)
                .Include(t => t.BorrowDetails)
                .OrderByDescending(t => t.BorrowDate)
                .Select(t => new
                {
                    t.Id,
                    ReaderName = t.Reader != null ? t.Reader.FullName : "",
                    t.BorrowDate,
                    t.DueDate,
                    t.ReturnDate,
                    t.Status,
                    t.Note,
                    Details = t.BorrowDetails!.Select(d => new
                    {
                        d.BookId,
                        d.Quantity,
                        d.Status
                    })
                })
                .ToListAsync();

            return Ok(history);
        }

        [HttpGet("reader/{readerId}")]
        public async Task<IActionResult> GetBorrowByReader(int readerId)
        {
            var result = await _context.BorrowTickets
                .Include(t => t.Reader)
                .Include(t => t.BorrowDetails)
                .Where(t => t.ReaderId == readerId)
                .OrderByDescending(t => t.BorrowDate)
                .ToListAsync();

            return Ok(result);
        }
    }
}