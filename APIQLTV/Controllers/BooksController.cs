using APIQLTV.DTOs.Books;
using APIQLTV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIQLTV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // Công khai: ai cũng xem được danh sách sách
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var books = await _bookService.GetAllBooksAsync();
            return Ok(books);
        }

        // Công khai: xem chi tiết sách
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        // Công khai: tìm kiếm sách
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var books = await _bookService.SearchBooksAsync(keyword);
            return Ok(books);
        }

        // 🔒 Admin và Librarian được tạo sách
        [HttpPost]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> Create([FromBody] BookCreateUpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var newBook = await _bookService.CreateBookAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = newBook.BookId }, newBook);
        }

        // 🔒 Admin và Librarian được cập nhật sách
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> Update(int id, [FromBody] BookCreateUpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _bookService.UpdateBookAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }

        // 🔒 Admin và Librarian được xóa sách
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _bookService.DeleteBookAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}