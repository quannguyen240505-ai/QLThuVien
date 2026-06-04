using AppQLTV.Models;

namespace AppQLTV.Services
{
    public interface IBorrowService
    {
        Task<BorrowRequest> CreateRequestAsync(int bookId, string? notes);
        Task<List<BorrowRequest>> GetMyRequestsAsync();
        Task<List<BorrowRequest>> GetPendingRequestsAsync();
        Task<List<BorrowRequest>> GetBorrowedRequestsAsync();   // ✅ thêm
        Task<bool> ApproveRequestAsync(int requestId, bool isApproved, string? rejectReason);
        Task<bool> ConfirmReturnAsync(int requestId);
        Task<List<BorrowRequest>> GetOverdueRequestsAsync();
    }
}