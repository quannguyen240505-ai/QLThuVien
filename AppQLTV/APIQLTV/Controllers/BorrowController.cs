using APIQLTV.DTOs.Borrow;
using APIQLTV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            var gmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(gmail))
                return Unauthorized("Không tìm thấy thông tin người dùng đăng nhập.");

            var reader = await _context.Readers
                .FirstOrDefaultAsync(r => r.Email == gmail);

            if (reader == null)
                return NotFound("Tài khoản này chưa có thông tin độc giả.");

            if (reader.Status != "Active")
                return BadRequest("Tài khoản độc giả của bạn đang bị ngưng hoạt động, không thể gửi yêu cầu mượn sách.");

            if (request.Books == null || request.Books.Count == 0)
                return BadRequest("Chưa chọn sách để mượn.");

            var totalQuantity = request.Books.Sum(x => x.Quantity);
            if (totalQuantity > 5)
                return BadRequest("Mỗi lần chỉ được mượn tối đa 5 quyển sách.");

            // Xử lý DueDate: nếu null thì mặc định +7 ngày, nếu có thì kiểm tra không quá 14 ngày
            var dueDate = request.DueDate?.Date ?? DateTime.Now.AddDays(7).Date;
            if (dueDate > DateTime.Now.Date.AddDays(14))
                return BadRequest("Số ngày mượn tối đa là 14 ngày.");

            var ticket = new BorrowTicket
            {
                ReaderId = reader.ReaderId,
                BorrowDate = DateTime.Now,
                DueDate = dueDate,  // đã là DateTime, không nullable
                Note = request.Note,
                Status = "Pending"
            };

            _context.BorrowTickets.Add(ticket);
            await _context.SaveChangesAsync();

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
                borrowTicketId = ticket.BorrowTicketId,
                readerName = reader.FullName
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
        //từ chối mượn 
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectBorrowRequest(int id)
        {
            var ticket = await _context.BorrowTickets
                .Include(t => t.BorrowDetails)
                .FirstOrDefaultAsync(t => t.BorrowTicketId == id);

            if (ticket == null)
                return NotFound("Không tìm thấy phiếu mượn.");

            if (ticket.Status != "Pending")
                return BadRequest("Chỉ có thể từ chối phiếu đang chờ duyệt.");

            ticket.Status = "Rejected";

            foreach (var detail in ticket.BorrowDetails)
            {
                detail.Status = "Rejected";
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã từ chối yêu cầu mượn sách." });
        }

        // Trả sách (dành cho thủ thư)
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
                overdueDays = (DateTime.Now.Date - ticket.DueDate.Date).Days;
                fineAmount = overdueDays * 5000;
            }

            foreach (var detail in ticket.BorrowDetails)
            {
                var book = await _context.Books.FindAsync(detail.BookId);
                if (book != null)
                    book.AvailableCopies += detail.Quantity;
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

        // Lịch sử mượn (tất cả)
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
        // Lịch sử mượn/trả của member đang đăng nhập
        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyBorrowHistory()
        {
            var gmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(gmail))
                return Unauthorized("Không tìm thấy thông tin người dùng.");

            var reader = await _context.Readers
                .FirstOrDefaultAsync(r => r.Email == gmail);

            if (reader == null)
                return NotFound("Không tìm thấy độc giả tương ứng với tài khoản.");

            var history = await _context.BorrowTickets
                .Include(t => t.Reader)
                .Include(t => t.BorrowDetails)
                .ThenInclude(d => d.Book)
                .Where(t => t.ReaderId == reader.ReaderId)
                .OrderByDescending(t => t.BorrowDate)
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
                    t.OverdueDays,
                    t.FineAmount,
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

        // Sách quá hạn
        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdueBooks()
        {
            var setting = await _context.LibrarySettings.FirstOrDefaultAsync();
            if (setting == null)
                return BadRequest("Chưa cấu hình thông tin thư viện.");

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
                .Select(t =>
                {
                    var overdueDays = (today - t.DueDate.Date).Days;
                    var fineAmount = overdueDays * setting.OverdueFinePerDay;
                    return new
                    {
                        t.BorrowTicketId,
                        ReaderName = t.Reader != null ? t.Reader.FullName : "",
                        t.BorrowDate,
                        t.DueDate,
                        OverdueDays = overdueDays,
                        FineAmount = fineAmount,
                        FinePerDay = setting.OverdueFinePerDay,
                        Details = t.BorrowDetails.Select(d => new
                        {
                            d.BookId,
                            BookTitle = d.Book != null ? d.Book.Title : "",
                            d.Quantity
                        }).ToList()
                    };
                })
                .ToList();

            return Ok(overdueBooks);
        }

        // Sách đang mượn của độc giả đang đăng nhập
        [HttpGet("my-borrowing")]
        public async Task<IActionResult> GetMyBorrowingBooks()
        {
            var gmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(gmail))
                return Unauthorized("Không tìm thấy thông tin người dùng.");

            var reader = await _context.Readers
                .FirstOrDefaultAsync(r => r.Email == gmail);
            if (reader == null)
                return NotFound("Không tìm thấy độc giả tương ứng với tài khoản.");

            var result = await _context.BorrowTickets
                .Include(t => t.BorrowDetails)
                .ThenInclude(d => d.Book)
                .Where(t => t.ReaderId == reader.ReaderId && t.Status == "Borrowing")
                .OrderByDescending(t => t.BorrowDate)
                .Select(t => new
                {
                    t.BorrowTicketId,
                    t.BorrowDate,
                    t.DueDate,
                    t.Status,
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

            return Ok(result);
        }

        // Thành viên tự trả sách
        [HttpPut("{id}/member-return")]
        public async Task<IActionResult> MemberReturnBook(int id)
        {
            var gmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(gmail))
                return Unauthorized("Không tìm thấy thông tin người dùng.");

            var reader = await _context.Readers
                .FirstOrDefaultAsync(r => r.Email == gmail);
            if (reader == null)
                return NotFound("Không tìm thấy độc giả tương ứng với tài khoản.");

            var ticket = await _context.BorrowTickets
                .Include(t => t.BorrowDetails)
                .FirstOrDefaultAsync(t =>
                    t.BorrowTicketId == id &&
                    t.ReaderId == reader.ReaderId);

            if (ticket == null)
                return NotFound("Bạn không có quyền trả phiếu mượn này.");

            if (ticket.Status != "Borrowing")
                return BadRequest("Phiếu này không ở trạng thái đang mượn.");

            var today = DateTime.Now.Date;
            int overdueDays = 0;
            int fineAmount = 0;

            if (ticket.DueDate.Date < today)
            {
                overdueDays = (today - ticket.DueDate.Date).Days;
                fineAmount = overdueDays * 5000;
            }

            foreach (var detail in ticket.BorrowDetails)
            {
                var book = await _context.Books.FindAsync(detail.BookId);
                if (book != null)
                    book.AvailableCopies += detail.Quantity;
                detail.Status = "Returned";
            }

            ticket.Status = "Returned";
            ticket.ReturnDate = DateTime.Now;
            ticket.OverdueDays = overdueDays;
            ticket.FineAmount = fineAmount;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Trả sách thành công.",
                overdueDays,
                fineAmount
            });
        }
    }
}