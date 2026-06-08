using APIQLTV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIQLTV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/books - Xem tất cả sách (PUBLIC - Không cần login)
        [HttpGet]
        [AllowAnonymous]   // Quan trọng: Cho phép xem khi chưa đăng nhập
        public async Task<ActionResult<IEnumerable<Book>>> GetAllBooks()
        {
            var books = await _context.Books
                .Where(b => b.IsActive)
                .OrderBy(b => b.Title)
                .ToListAsync();

            return Ok(books);
        }

        // GET: api/books/{id} - Xem chi tiết sách (PUBLIC)
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null || !book.IsActive)
                return NotFound("Không tìm thấy sách.");

            return Ok(book);
        }

        // ================== Dành cho Thủ thư / Admin ==================

        // POST: api/books - Thêm sách mới (Chỉ Thủ thư/Admin)
        [HttpPost]
        [Authorize(Roles = "Librarian,Admin")]
        public async Task<ActionResult<Book>> CreateBook(Book book)
        {
            book.AvailableCopies = book.TotalCopies;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }

        // PUT: api/books/{id} - Cập nhật sách (Thủ thư)
        [HttpPut("{id}")]
        [Authorize(Roles = "Librarian,Admin")]
        public async Task<IActionResult> UpdateBook(int id, Book book)
        {
            if (id != book.Id) return BadRequest();

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/books/{id} - Xóa sách (Thủ thư)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Librarian,Admin")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            book.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
        // GET: api/books/search?keyword=... - Tìm kiếm sách
        [HttpGet("search")]
        [AllowAnonymous] // Cho phép cả người chưa đăng nhập
        public async Task<ActionResult<IEnumerable<Book>>> SearchBooks([FromQuery] string keyword)
        {
            // 1. Nếu không có từ khóa thì trả về danh sách trống
            if (string.IsNullOrWhiteSpace(keyword))
                return Ok(new List<Book>());

            // 2. Tìm kiếm theo Title, Author hoặc Category
            var books = await _context.Books
                .Where(b => b.IsActive &&
                            (b.Title.Contains(keyword) ||
                             b.Author.Contains(keyword) ||
                             (b.Category != null && b.Category.Contains(keyword))))
                .OrderBy(b => b.Title)
                .ToListAsync();

            return Ok(books);
        }
    }
}