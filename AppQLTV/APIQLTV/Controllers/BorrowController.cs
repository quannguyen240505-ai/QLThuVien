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
            try
            {
                // Validation tổng số lượng
                var totalQuantity = request.Books.Sum(x => x.Quantity);
                if (totalQuantity > 5)
                    return BadRequest("Mỗi lần chỉ được mượn tối đa 5 quyển sách.");

                // Xử lý DueDate an toàn
                var dueDate = request.DueDate?.Date ?? DateTime.Now.AddDays(7).Date;
                var maxDueDate = DateTime.Now.Date.AddDays(14);
                if (dueDate > maxDueDate)
                    return BadRequest("Số ngày mượn tối đa là 14 ngày.");

                // Kiểm tra độc giả
                var reader = await _context.Readers.FindAsync(request.ReaderId);
                if (reader == null)
                    return NotFound("Không tìm thấy độc giả.");

                if (request.Books == null || request.Books.Count == 0)
                    return BadRequest("Chưa chọn sách để mượn.");

                // Kiểm tra trước tất cả sách
                foreach (var item in request.Books)
                {
                    var book = await _context.Books.FindAsync(item.BookId);
                    if (book == null)
                        return BadRequest($"Không tìm thấy sách ID = {item.BookId}");
                    if (book.AvailableCopies < item.Quantity)
                        return BadRequest($"Sách {book.Title} không đủ số lượng.");
                }

                // Tạo phiếu mượn
                var ticket = new BorrowTicket
                {
                    ReaderId = request.ReaderId,
                    BorrowDate = DateTime.Now,
                    DueDate = dueDate,
                    Note = request.Note,
                    Status = "Pending"
                };

                _context.BorrowTickets.Add(ticket);
                await _context.SaveChangesAsync();  // Lưu để lấy BorrowTicketId

                // Tạo chi tiết mượn
                foreach (var item in request.Books)
                {
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
            catch (DbUpdateException ex)
            {
                // Lấy inner exception chi tiết
                var inner = ex.InnerException?.Message ?? ex.Message;
                return BadRequest($"Lỗi database: {inner}");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.Message}");
            }
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
                return BadRequest("Phiếu này chưa được duyệt.");

            int overdueDays = 0;
            int fineAmount = 0;

            if (DateTime.Now.Date > ticket.DueDate.Date)
            {
                overdueDays =
                    (DateTime.Now.Date - ticket.DueDate.Date).Days;

                fineAmount = overdueDays * 5000;
            }

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

            return Ok(new
            {
                message = "Trả sách thành công",
                overdueDays,
                fineAmount
            });
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
        // sách quá hạn 
        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdueBooks()
        {
            var today = DateTime.Now.Date;

            var data = await _context.BorrowTickets
                .Include(t => t.Reader)
                .Include(t => t.BorrowDetails)
                .ThenInclude(d => d.Book)
                .Where(t => t.Status == "Borrowing")
                .ToListAsync();

            var overdueBooks = data
                .Where(t => t.DueDate.Date < today)
                .OrderBy(t => t.DueDate)
                .Select(t => new
                {
                    t.BorrowTicketId,
                    ReaderName = t.Reader != null ? t.Reader.FullName : "",
                    t.BorrowDate,
                    t.DueDate,
                    OverdueDays = (today - t.DueDate.Date).Days,
                    FineAmount = (today - t.DueDate.Date).Days * 5000,
                    Details = t.BorrowDetails.Select(d => new
                    {
                        d.BookId,
                        BookTitle = d.Book != null ? d.Book.Title : "",
                        d.Quantity
                    }).ToList()
                })
                .ToList();

            return Ok(overdueBooks);
        }
    }
}