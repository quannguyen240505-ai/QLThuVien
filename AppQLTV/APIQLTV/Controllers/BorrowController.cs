using APIQLTV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // POST: api/Borrow - Tạo phiếu mượn mới
        [HttpPost]
        [Authorize(Roles = "Librarian,Admin,Member")]
        public async Task<IActionResult> CreateBorrowTicket([FromBody] CreateBorrowRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reader = await _context.Readers.FindAsync(request.ReaderId);
            if (reader == null)
                return NotFound("Độc giả không tồn tại.");

            foreach (var item in request.Books)
            {
                var book = await _context.Books.FindAsync(item.BookId);
                if (book == null)
                    return NotFound($"Sách có ID {item.BookId} không tồn tại.");
                if (book.AvailableCopies < item.Quantity)
                    return BadRequest($"Sách '{book.Title}' không đủ số lượng (còn {book.AvailableCopies}).");
            }

            var ticket = new BorrowTicket
            {
                ReaderId = request.ReaderId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(request.DueDays > 0 ? request.DueDays : 14),
                Status = "Borrowing",
                Note = request.Note
            };

            _context.BorrowTickets.Add(ticket);
            await _context.SaveChangesAsync();

            foreach (var item in request.Books)
            {
                var book = await _context.Books.FindAsync(item.BookId);
                book.AvailableCopies -= item.Quantity;

                var detail = new BorrowDetail
                {
                    BorrowTicketId = ticket.BorrowTicketId,
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    Status = "Borrowing"
                };
                _context.BorrowDetails.Add(detail);
            }

            await _context.SaveChangesAsync();
            return Ok(new { ticketId = ticket.BorrowTicketId, message = "Tạo phiếu mượn thành công." });
        }

        // PUT: api/Borrow/{id}/return - Xác nhận trả sách
        [HttpPut("{id}/return")]
        [Authorize(Roles = "Librarian,Admin")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var ticket = await _context.BorrowTickets
                .Include(bt => bt.BorrowDetails)
                .FirstOrDefaultAsync(bt => bt.BorrowTicketId == id);

            if (ticket == null)
                return NotFound("Phiếu mượn không tồn tại.");

            if (ticket.Status != "Borrowing")
                return BadRequest("Sách đã được trả hoặc không trong trạng thái mượn.");

            ticket.ReturnDate = DateTime.Now;
            ticket.Status = "Returned";

            foreach (var detail in ticket.BorrowDetails)
            {
                var book = await _context.Books.FindAsync(detail.BookId);
                if (book != null)
                {
                    book.AvailableCopies += detail.Quantity;
                }
                detail.Status = "Returned";
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Trả sách thành công" });
        }

        // GET: api/Borrow/history - Lấy lịch sử mượn/trả (có phân trang, lọc)
        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetBorrowHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? readerName = null, [FromQuery] string? status = null)
        {
            var query = from bt in _context.BorrowTickets
                        join r in _context.Readers on bt.ReaderId equals r.ReaderId
                        select new { bt, r };

            if (!string.IsNullOrEmpty(readerName))
                query = query.Where(x => x.r.FullName.Contains(readerName));
            if (!string.IsNullOrEmpty(status))
                query = query.Where(x => x.bt.Status == status);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.bt.BorrowDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.bt.BorrowTicketId,
                    ReaderName = x.r.FullName,
                    BorrowDate = x.bt.BorrowDate,
                    DueDate = x.bt.DueDate,
                    ReturnDate = x.bt.ReturnDate,
                    Status = x.bt.Status,
                    Books = _context.BorrowDetails
                        .Where(bd => bd.BorrowTicketId == x.bt.BorrowTicketId)
                        .Join(_context.Books, bd => bd.BookId, b => b.Id, (bd, b) => new { bd.Quantity, b.Title })
                        .ToList()
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        // GET: api/Borrow/reader/{readerId} - Lấy lịch sử mượn của một độc giả
        [HttpGet("reader/{readerId}")]
        [Authorize]
        public async Task<IActionResult> GetBorrowByReader(int readerId)
        {
            var reader = await _context.Readers.FindAsync(readerId);
            if (reader == null)
                return NotFound("Độc giả không tồn tại.");

            var history = await (from bt in _context.BorrowTickets
                                 where bt.ReaderId == readerId
                                 join r in _context.Readers on bt.ReaderId equals r.ReaderId
                                 orderby bt.BorrowDate descending
                                 select new
                                 {
                                     bt.BorrowTicketId,
                                     BorrowDate = bt.BorrowDate,
                                     DueDate = bt.DueDate,
                                     ReturnDate = bt.ReturnDate,
                                     Status = bt.Status,
                                     Books = _context.BorrowDetails
                                         .Where(bd => bd.BorrowTicketId == bt.BorrowTicketId)
                                         .Join(_context.Books, bd => bd.BookId, b => b.Id, (bd, b) => new { bd.Quantity, b.Title })
                                         .ToList()
                                 }).ToListAsync();

            return Ok(history);
        }

        // GET: api/Borrow/pending - Lấy danh sách phiếu mượn đang chờ (dành cho Librarian/Admin)
        [HttpGet("pending")]
        [Authorize(Roles = "Librarian,Admin")]
        public async Task<IActionResult> GetPendingBorrows()
        {
            var pending = await (
                from bt in _context.BorrowTickets
                join r in _context.Readers on bt.ReaderId equals r.ReaderId
                join bd in _context.BorrowDetails on bt.BorrowTicketId equals bd.BorrowTicketId
                join b in _context.Books on bd.BookId equals b.Id
                where bt.Status == "Borrowing" && bt.ReturnDate == null
                select new
                {
                    Id = bt.BorrowTicketId,
                    ReaderName = r.FullName,
                    BookTitle = b.Title,
                    BorrowDate = bt.BorrowDate,
                    DueDate = bt.DueDate,
                    Status = bt.Status
                })
                .ToListAsync();

            return Ok(pending);
        }
    }

    public class CreateBorrowRequest
    {
        public int ReaderId { get; set; }
        public List<BorrowBookItem> Books { get; set; } = new();
        public int DueDays { get; set; } = 14;
        public string? Note { get; set; }
    }

    public class BorrowBookItem
    {
        public int BookId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}