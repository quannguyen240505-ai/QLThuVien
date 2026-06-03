using APIQLTV.Models;
using Microsoft.EntityFrameworkCore;

namespace APIQLTV.Services
{
    public class LibrarySettingService
    {
        private readonly AppDbContext _context;

        public LibrarySettingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LibrarySetting> GetCurrentSettingAsync()
        {
            var setting = await _context.LibrarySettings.FirstOrDefaultAsync();

            if (setting != null)
            {
                return setting;
            }

            setting = new LibrarySetting
            {
                LibraryName = "QNU Library",
                Email = "library@qnu.edu.vn",
                Phone = "087 9915 327",
                Address = "Trường Đại học Quy Nhơn",
                OpeningHours = "07:30 - 22:30",
                MaxBorrowBooks = 5,
                MaxBorrowDays = 14,
                OverdueFinePerDay = 5000,
                AllowBorrowRequest = true,
                LibraryRules = "Sinh viên cần giữ gìn tài liệu, trả sách đúng hạn và tuân thủ nội quy thư viện.",
                UpdatedAt = DateTime.Now
            };

            _context.LibrarySettings.Add(setting);
            await _context.SaveChangesAsync();

            return setting;
        }

        public async Task<bool> IsBorrowRequestAllowedAsync()
        {
            var setting = await GetCurrentSettingAsync();

            return setting.AllowBorrowRequest;
        }

        public async Task<int> GetMaxBorrowBooksAsync()
        {
            var setting = await GetCurrentSettingAsync();

            return setting.MaxBorrowBooks;
        }

        public async Task<int> GetMaxBorrowDaysAsync()
        {
            var setting = await GetCurrentSettingAsync();

            return setting.MaxBorrowDays;
        }

        public async Task<decimal> GetOverdueFinePerDayAsync()
        {
            var setting = await GetCurrentSettingAsync();

            return setting.OverdueFinePerDay;
        }

        public async Task<DateTime> CalculateDueDateAsync(DateTime borrowDate)
        {
            var setting = await GetCurrentSettingAsync();

            return borrowDate.AddDays(setting.MaxBorrowDays);
        }

        public async Task<decimal> CalculateOverdueFineAsync(DateTime dueDate, DateTime returnDate)
        {
            var setting = await GetCurrentSettingAsync();

            int overdueDays = (returnDate.Date - dueDate.Date).Days;

            if (overdueDays <= 0)
            {
                return 0;
            }

            return overdueDays * setting.OverdueFinePerDay;
        }
    }
}