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

        // Gửi yêu cầu mượn sách - trạng thái Pending
        [HttpPost("request")]
        public async Task<IActionResult> SendBorrowRequest(BorrowRequest request)
        {
            var reader = await _context.Readers.FindAsync(request.ReaderId);

            if (reader == null)
                return NotFound("Không tìm thấy độc giả.");

            if (request.Books == null || request.Books.Count == 0)
                return BadRequest("Chưa chọn sách để mượn.");

            var ticket = new BorrowTicket
            {
                ReaderId = request.ReaderId,
                BorrowDate = DateTime.Now,
                DueDate = request.DueDate,
                Note = request.Note,
                Status = "Pending"
            };

            _context.BorrowTickets.Add(ticket);
            await _context.SaveChangesAsync();

            foreach (var item in request.Books)
            {
                var book = await _context.Books.FindAsync(item.BookId);

                if (book == null)
                    return BadRequest($"Không tìm thấy sách ID = {item.BookId}");

                if (book.AvailableCopies < item.Quantity)
                    return BadRequest($"Sách {book.Title} không đủ số lượng.");

                var detail = new BorrowDetail
                {
                    BorrowTicketId = ticket.BorrowTicketId,
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    Status = "Pending"
                };

                _context.BorrowDetails.Add(detail);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Gửi yêu cầu mượn sách thành công.",
                borrowTicketId = ticket.BorrowTicketId
            });
        }

        // Duyệt yêu cầu mượn sách - Pending -> Borrowing
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveBorrowRequest(int id)
        {
            var ticket = await _context.BorrowTickets
                .Include(t => t.BorrowDetails)
                .FirstOrDefaultAsync(t => t.BorrowTicketId == id);

            if (ticket == null)
                return NotFound("Không tìm thấy phiếu mượn.");

            if (ticket.Status != "Pending")
                return BadRequest("Phiếu này không ở trạng thái chờ duyệt.");

            if (ticket.BorrowDetails == null || ticket.BorrowDetails.Count == 0)
                return BadRequest("Phiếu mượn không có chi tiết sách.");

            foreach (var detail in ticket.BorrowDetails)
            {
                var book = await _context.Books.FindAsync(detail.BookId);

                if (book == null)
                    return BadRequest($"Không tìm thấy sách ID = {detail.BookId}");

                if (book.AvailableCopies < detail.Quantity)
                    return BadRequest($"Sách {book.Title} không đủ số lượng.");

                book.AvailableCopies -= detail.Quantity;
                detail.Status = "Borrowing";
            }

            ticket.Status = "Borrowing";

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã duyệt yêu cầu mượn sách." });
        }

        // Trả sách
        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnBooks(int id)
        {
            var ticket = await _context.BorrowTickets
                .Include(t => t.BorrowDetails)
                .FirstOrDefaultAsync(t => t.BorrowTicketId == id);

            if (ticket == null)
                return NotFound("Không tìm thấy phiếu mượn.");

            if (ticket.Status == "Returned")
                return BadRequest("Phiếu này đã được trả trước đó.");

            if (ticket.Status == "Pending")
                return BadRequest("Phiếu này chưa được duyệt, không thể trả sách.");

            if (ticket.BorrowDetails == null)
                return BadRequest("Phiếu mượn không có chi tiết sách.");

            foreach (var detail in ticket.BorrowDetails)
            {
                var book = await _context.Books.FindAsync(detail.BookId);

                if (book != null)
                {
                    book.AvailableCopies += detail.Quantity;
                }

                detail.Status = "Returned";
            }

            ticket.Status = "Returned";
            ticket.ReturnDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Trả sách thành công." });
        }

        // Lịch sử mượn
        [HttpGet("history")]
        public async Task<IActionResult> GetBorrowHistory()
        {
            var history = await _context.BorrowTickets
                .Include(t => t.Reader)
                .Include(t => t.BorrowDetails)
                .ThenInclude(d => d.Book)
                .OrderBy(t => t.BorrowDate)
                .Select(t => new
                {
                    t.BorrowTicketId,
                    ReaderId = t.ReaderId,
                    ReaderName = t.Reader != null ? t.Reader.FullName : "",
                    t.BorrowDate,
                    t.DueDate,
                    t.ReturnDate,
                    t.Status,
                    t.Note,
                    Details = t.BorrowDetails.Select(d => new
                    {
                        d.BorrowDetailId,
                        d.BookId,
                        BookTitle = d.Book != null ? d.Book.Title : "",
                        d.Quantity,
                        d.Status
                    }).ToList()
                })
                .ToListAsync();

            return Ok(history);
        }

        // Lịch sử theo độc giả
        [HttpGet("reader/{readerId}")]
        public async Task<IActionResult> GetBorrowByReader(int readerId)
        {
            var result = await _context.BorrowTickets
                .Include(t => t.Reader)
                .Include(t => t.BorrowDetails)
                .ThenInclude(d => d.Book)
                .Where(t => t.ReaderId == readerId)
                .OrderBy(t => t.BorrowDate)
                .ToListAsync();

            return Ok(result);
        }
    }
}