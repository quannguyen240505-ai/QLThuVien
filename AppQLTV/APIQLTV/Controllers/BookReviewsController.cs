using APIQLTV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APIQLTV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookReviewsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/BookReviews/book/{bookId} - công khai
        [HttpGet("book/{bookId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetReviewsByBook(int bookId)
        {
            var reviews = await _context.BookReviews
                .Where(r => r.BookId == bookId && r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.ReviewId,
                    r.BookId,
                    r.Comment,
                    r.Rating,
                    r.CreatedAt,
                    UserName = _context.Users.Where(u => u.Id.ToString() == r.UserId).Select(u => u.Username).FirstOrDefault() ?? "Người dùng"
                })
                .ToListAsync();
            return Ok(reviews);
        }

        // POST: api/BookReviews - yêu cầu đăng nhập
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Vui lòng đăng nhập.");

            var book = await _context.Books.FindAsync(dto.BookId);
            if (book == null)
                return NotFound("Sách không tồn tại.");

            var review = new BookReview
            {
                BookId = dto.BookId,
                UserId = userId,
                Comment = dto.Comment,
                Rating = dto.Rating,
                CreatedAt = DateTime.Now,
                IsApproved = true   // Có thể set true hoặc cần duyệt
            };
            _context.BookReviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đánh giá thành công." });
        }

        // GET: api/BookReviews/admin/all - dành cho thủ thư
        [HttpGet("admin/all")]
        [Authorize(Roles = "Librarian,Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllReviewsForAdmin()
        {
            var reviews = await _context.BookReviews
                .Include(r => r.Book)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.ReviewId,
                    BookTitle = r.Book != null ? r.Book.Title : "",
                    UserName = _context.Users.Where(u => u.Id.ToString() == r.UserId).Select(u => u.Username).FirstOrDefault() ?? "Người dùng",
                    r.Comment,
                    r.Rating,
                    r.CreatedAt,
                    r.IsApproved
                })
                .ToListAsync();
            return Ok(reviews);
        }

        // DELETE: api/BookReviews/{id} - thủ thư
        [HttpDelete("{id}")]
        [Authorize(Roles = "Librarian,Admin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.BookReviews.FindAsync(id);
            if (review == null) return NotFound();
            _context.BookReviews.Remove(review);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa bình luận thành công." });
        }

        // PUT: api/BookReviews/{id}/toggle-approve - thủ thư
        [HttpPut("{id}/toggle-approve")]
        [Authorize(Roles = "Librarian,Admin")]
        public async Task<IActionResult> ToggleApprove(int id)
        {
            var review = await _context.BookReviews.FindAsync(id);
            if (review == null) return NotFound();
            review.IsApproved = !review.IsApproved;
            await _context.SaveChangesAsync();
            return Ok(new { message = review.IsApproved ? "Đã hiện bình luận" : "Đã ẩn bình luận" });
        }
    }

    public class CreateReviewDto
    {
        public int BookId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }
    }
}