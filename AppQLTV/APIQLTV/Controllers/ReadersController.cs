using APIQLTV.DTOs.Reader;
using APIQLTV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIQLTV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReadersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReadersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Readers
        [HttpGet]
        public async Task<IActionResult> GetReaders()
        {
            var readers = await _context.Readers
                .OrderBy(r => r.CreatedDate)
                .ThenBy(r => r.ReaderId)
                .ToListAsync();

            return Ok(readers);
        }

        // GET: api/Readers/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReader(int id)
        {
            var reader = await _context.Readers.FindAsync(id);

            if (reader == null)
                return NotFound("Không tìm thấy độc giả.");

            return Ok(reader);
        }

        // GET: api/Readers/search?keyword=sang
        [HttpGet("search")]
        public async Task<IActionResult> SearchReaders([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                var allReaders = await _context.Readers
                    .OrderBy(r => r.CreatedDate)
                    .ThenBy(r => r.ReaderId)
                    .ToListAsync();

                return Ok(allReaders);
            }

            keyword = keyword.Trim();

            var readers = await _context.Readers
                .Where(r =>
                    r.FullName.Contains(keyword) ||
                    (r.Email != null && r.Email.Contains(keyword)) ||
                    (r.Phone != null && r.Phone.Contains(keyword)) ||
                    (r.Address != null && r.Address.Contains(keyword)))
                .OrderBy(r => r.CreatedDate)
                .ThenBy(r => r.ReaderId)
                .ToListAsync();

            return Ok(readers);
        }

        // POST: api/Readers
        [HttpPost]
        public async Task<IActionResult> CreateReader([FromBody] ReaderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                return BadRequest("Họ tên độc giả không được để trống.");

            var reader = new Reader
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                CreatedDate = DateTime.Now,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status
            };

            _context.Readers.Add(reader);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Thêm độc giả thành công.",
                reader
            });
        }

        // PUT: api/Readers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReader(int id, [FromBody] ReaderRequest request)
        {
            var reader = await _context.Readers.FindAsync(id);

            if (reader == null)
                return NotFound("Không tìm thấy độc giả.");

            reader.FullName = request.FullName;
            reader.Email = request.Email;
            reader.Phone = request.Phone;
            reader.Address = request.Address;
            reader.DateOfBirth = request.DateOfBirth;
            reader.Gender = request.Gender;
            reader.Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật độc giả thành công.",
                reader
            });
        }

        // DELETE: api/Readers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReader(int id)
        {
            var reader = await _context.Readers.FindAsync(id);

            if (reader == null)
                return NotFound("Không tìm thấy độc giả.");

            _context.Readers.Remove(reader);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Xóa độc giả thành công."
            });
        }
    }
}