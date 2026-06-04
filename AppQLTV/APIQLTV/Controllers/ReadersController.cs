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

        [HttpGet]
        public async Task<IActionResult> GetReaders()
        {
            var readers = await _context.Readers
                .OrderByDescending(r => r.ReaderId)
                .ToListAsync();

            return Ok(readers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReader(int id)
        {
            var reader = await _context.Readers.FindAsync(id);

            if (reader == null)
                return NotFound("Không tìm thấy độc giả");

            return Ok(reader);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReader(ReaderRequest request)
        {
            var reader = new Reader
            {
                FullName = request.FullName,
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth,
                Phone = request.Phone,
                Email = request.Email,
                Address = request.Address,
                Status = request.Status
            };

            _context.Readers.Add(reader);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Thêm độc giả thành công",
                reader
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReader(int id, ReaderRequest request)
        {
            var reader = await _context.Readers.FindAsync(id);

            if (reader == null)
                return NotFound("Không tìm thấy độc giả");

            reader.FullName = request.FullName;
            reader.Gender = request.Gender;
            reader.DateOfBirth = request.DateOfBirth;
            reader.Phone = request.Phone;
            reader.Email = request.Email;
            reader.Address = request.Address;
            reader.Status = request.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật độc giả thành công",
                reader
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReader(int id)
        {
            var reader = await _context.Readers.FindAsync(id);

            if (reader == null)
                return NotFound("Không tìm thấy độc giả");

            _context.Readers.Remove(reader);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa độc giả thành công" });
        }
    }
}