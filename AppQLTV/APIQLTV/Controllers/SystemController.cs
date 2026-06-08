using APIQLTV.DTOs.Admin;
using APIQLTV.Models;
using APIQLTV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIQLTV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly LibrarySettingService _librarySettingService;

        public SystemController(
            AppDbContext context,
            LibrarySettingService librarySettingService)
        {
            _context = context;
            _librarySettingService = librarySettingService;
        }

        // GET: api/System/settings-public
        // Guest / Member / Librarian / Admin đều đọc được
        [HttpGet("settings-public")]
        [AllowAnonymous]
        public async Task<ActionResult<LibrarySettingResponse>> GetPublicSettings()
        {
            var setting = await _librarySettingService.GetCurrentSettingAsync();

            return Ok(MapToResponse(setting));
        }

        // GET: api/System/settings
        // Chỉ Admin dùng trong trang quản lý hệ thống
        [HttpGet("settings")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<LibrarySettingResponse>> GetSettings()
        {
            var setting = await _librarySettingService.GetCurrentSettingAsync();

            return Ok(MapToResponse(setting));
        }

        // PUT: api/System/settings
        // Chỉ Admin được cập nhật cấu hình
        [HttpPut("settings")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSettings(UpdateLibrarySettingRequest request)
        {
            var setting = await _librarySettingService.GetCurrentSettingAsync();

            setting.LibraryName = request.LibraryName;
            setting.Email = request.Email;
            setting.Phone = request.Phone;
            setting.Address = request.Address;
            setting.OpeningHours = request.OpeningHours;
            setting.MaxBorrowBooks = request.MaxBorrowBooks;
            setting.MaxBorrowDays = request.MaxBorrowDays;
            setting.OverdueFinePerDay = request.OverdueFinePerDay;
            setting.AllowBorrowRequest = request.AllowBorrowRequest;
            setting.LibraryRules = request.LibraryRules;
            setting.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok("Cập nhật cấu hình hệ thống thành công.");
        }

        private LibrarySettingResponse MapToResponse(LibrarySetting setting)
        {
            return new LibrarySettingResponse
            {
                Id = setting.Id,
                LibraryName = setting.LibraryName,
                Email = setting.Email,
                Phone = setting.Phone,
                Address = setting.Address,
                OpeningHours = setting.OpeningHours,
                MaxBorrowBooks = setting.MaxBorrowBooks,
                MaxBorrowDays = setting.MaxBorrowDays,
                OverdueFinePerDay = setting.OverdueFinePerDay,
                AllowBorrowRequest = setting.AllowBorrowRequest,
                LibraryRules = setting.LibraryRules,
                UpdatedAt = setting.UpdatedAt
            };
        }
    }
}