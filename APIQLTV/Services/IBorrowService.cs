using System.Collections.Generic;
using System.Threading.Tasks;
using APIQLTV.DTOs.Borrow;

namespace APIQLTV.Services
{
    public interface IBorrowService
    {
        Task<BorrowRequestResponseDTO> CreateRequestAsync(int userId, BorrowRequestCreateDTO dto);
        Task<List<BorrowRequestResponseDTO>> GetRequestsByUserAsync(int userId);
        Task<List<BorrowRequestResponseDTO>> GetAllPendingRequestsAsync();
        Task<List<BorrowRequestResponseDTO>> GetBorrowedRequestsAsync(); // thêm
        Task<bool> ApproveRequestAsync(int requestId, bool isApproved, string? rejectReason);
        Task<bool> ConfirmReturnAsync(int requestId);
        Task<List<BorrowRequestResponseDTO>> GetOverdueRequestsAsync();
    }
}