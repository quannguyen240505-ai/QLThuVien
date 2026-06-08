using APIQLTV.DTOs.Admin;
using APIQLTV.Models;
using APIQLTV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIQLTV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class StatisticsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly LibrarySettingService _librarySettingService;

        public StatisticsController(
            AppDbContext context,
            LibrarySettingService librarySettingService)
        {
            _context = context;
            _librarySettingService = librarySettingService;
        }

        // GET: api/Statistics/activity
        [HttpGet("activity")]
        public async Task<ActionResult<ActivityStatisticsResponse>> GetActivityStatistics()
        {
            var setting = await _librarySettingService.GetCurrentSettingAsync();

            var statistics = new ActivityStatisticsResponse
            {
                TotalUsers = await _context.Users.CountAsync(),

                TotalMembers = await _context.Users.CountAsync(u => u.Role == "Member"),

                TotalLibrarians = await _context.Users.CountAsync(u => u.Role == "Librarian"),

                TotalAdmins = await _context.Users.CountAsync(u => u.Role == "Admin"),

                TotalActiveUsers = await _context.Users.CountAsync(u => u.IsActive),

                TotalLockedUsers = await _context.Users.CountAsync(u => !u.IsActive),

                TotalLocalAccounts = await _context.Users.CountAsync(u => u.AuthProvider == "Local"),

                TotalGoogleAccounts = await _context.Users.CountAsync(u => u.AuthProvider == "Google"),

                LibraryName = setting.LibraryName,

                AllowBorrowRequest = setting.AllowBorrowRequest,

                MaxBorrowBooks = setting.MaxBorrowBooks,

                MaxBorrowDays = setting.MaxBorrowDays,

                OverdueFinePerDay = setting.OverdueFinePerDay,

                UpdatedAt = setting.UpdatedAt
            };

            return Ok(statistics);
        }
    }
}