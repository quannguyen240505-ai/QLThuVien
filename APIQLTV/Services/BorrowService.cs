using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIQLTV.Models;
using APIQLTV.DTOs.Borrow;
using Microsoft.EntityFrameworkCore;

namespace APIQLTV.Services
{
    public class BorrowService : IBorrowService
    {
        private readonly AppDbContext _context;

        public BorrowService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BorrowRequestResponseDTO> CreateRequestAsync(int userId, BorrowRequestCreateDTO dto)
        {
            var book = await _context.Books.FindAsync(dto.BookId);
            if (book == null) throw new Exception("Sách không tồn tại.");
            if (book.AvailableQuantity <= 0) throw new Exception("Sách đã hết, không thể mượn.");

            var existing = await _context.BorrowRequests
                .AnyAsync(r => r.UserId == userId && r.BookId == dto.BookId && (r.Status == "Borrowed" || r.Status == "Pending"));
            if (existing) throw new Exception("Bạn đã có yêu cầu hoặc đang mượn sách này.");

            var request = new BorrowRequest
            {
                UserId = userId,
                BookId = dto.BookId,
                RequestDate = DateTime.Now,
                Status = "Pending",
                Notes = dto.Notes
            };
            _context.BorrowRequests.Add(request);
            await _context.SaveChangesAsync();

            return await MapToResponse(request);
        }

        public async Task<List<BorrowRequestResponseDTO>> GetRequestsByUserAsync(int userId)
        {
            var requests = await _context.BorrowRequests
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            var result = new List<BorrowRequestResponseDTO>();
            foreach (var r in requests) result.Add(await MapToResponse(r));
            return result;
        }

        public async Task<List<BorrowRequestResponseDTO>> GetAllPendingRequestsAsync()
        {
            var requests = await _context.BorrowRequests
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.Status == "Pending")
                .OrderBy(r => r.RequestDate)
                .ToListAsync();

            var result = new List<BorrowRequestResponseDTO>();
            foreach (var r in requests) result.Add(await MapToResponse(r));
            return result;
        }

        public async Task<List<BorrowRequestResponseDTO>> GetBorrowedRequestsAsync()
        {
            var requests = await _context.BorrowRequests
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.Status == "Borrowed")
                .OrderBy(r => r.DueDate)
                .ToListAsync();

            var result = new List<BorrowRequestResponseDTO>();
            foreach (var r in requests) result.Add(await MapToResponse(r));
            return result;
        }

        public async Task<bool> ApproveRequestAsync(int requestId, bool isApproved, string? rejectReason)
        {
            var request = await _context.BorrowRequests.FindAsync(requestId);
            if (request == null) throw new Exception("Yêu cầu không tồn tại.");
            if (request.Status != "Pending") throw new Exception("Yêu cầu đã được xử lý trước đó.");

            if (isApproved)
            {
                var book = await _context.Books.FindAsync(request.BookId);
                if (book == null) throw new Exception("Sách không tồn tại.");
                if (book.AvailableQuantity <= 0) throw new Exception("Sách hiện không còn để mượn.");

                request.Status = "Borrowed";
                request.ApproveDate = DateTime.Now;
                request.DueDate = DateTime.Now.AddDays(7);
                book.AvailableQuantity--;
                _context.Books.Update(book);
            }
            else
            {
                request.Status = "Rejected";
                request.Notes = rejectReason ?? "Từ chối bởi thủ thư";
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ConfirmReturnAsync(int requestId)
        {
            var request = await _context.BorrowRequests.FindAsync(requestId);
            if (request == null) throw new Exception("Yêu cầu không tồn tại.");
            if (request.Status != "Borrowed") throw new Exception("Chỉ có thể trả sách khi đang mượn.");

            request.Status = "Returned";
            request.ReturnDate = DateTime.Now;

            var book = await _context.Books.FindAsync(request.BookId);
            if (book != null)
            {
                book.AvailableQuantity++;
                _context.Books.Update(book);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<BorrowRequestResponseDTO>> GetOverdueRequestsAsync()
        {
            var today = DateTime.Now.Date;
            var requests = await _context.BorrowRequests
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.Status == "Borrowed" && r.DueDate < today)
                .OrderBy(r => r.DueDate)
                .ToListAsync();

            var result = new List<BorrowRequestResponseDTO>();
            foreach (var r in requests) result.Add(await MapToResponse(r));
            return result;
        }

        private async Task<BorrowRequestResponseDTO> MapToResponse(BorrowRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            var book = await _context.Books.FindAsync(request.BookId);
            return new BorrowRequestResponseDTO
            {
                Id = request.Id,
                UserId = request.UserId,
                UserName = user?.Username ?? "N/A",
                BookId = request.BookId,
                BookTitle = book?.Title ?? "N/A",
                RequestDate = request.RequestDate,
                Status = request.Status,
                ApproveDate = request.ApproveDate,
                ReturnDate = request.ReturnDate,
                DueDate = request.DueDate,
                Notes = request.Notes
            };
        }
    }
}