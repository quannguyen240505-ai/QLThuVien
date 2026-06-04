using System.Security.Claims;
using System.Threading.Tasks;
using APIQLTV.DTOs.Borrow;
using APIQLTV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIQLTV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowController : ControllerBase
    {
        private readonly IBorrowService _borrowService;

        public BorrowController(IBorrowService borrowService)
        {
            _borrowService = borrowService;
        }

        // Member: gửi yêu cầu mượn
        [HttpPost]
        [Authorize(Roles = "Member, Librarian, Admin")] // Member là chính
        public async Task<IActionResult> CreateRequest([FromBody] BorrowRequestCreateDTO dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _borrowService.CreateRequestAsync(userId.Value, dto);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Member: xem lịch sử mượn của mình
        [HttpGet("my-requests")]
        [Authorize(Roles = "Member, Librarian, Admin")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            var requests = await _borrowService.GetRequestsByUserAsync(userId.Value);
            return Ok(requests);
        }

        // Librarian/Admin: lấy danh sách pending
        [HttpGet("pending")]
        [Authorize(Roles = "Librarian, Admin")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var requests = await _borrowService.GetAllPendingRequestsAsync();
            return Ok(requests);
        }

        // Librarian/Admin: duyệt/từ chối yêu cầu
        [HttpPut("approve")]
        [Authorize(Roles = "Librarian, Admin")]
        public async Task<IActionResult> ApproveRequest([FromBody] BorrowRequestApproveDTO dto)
        {
            try
            {
                var result = await _borrowService.ApproveRequestAsync(dto.Id, dto.IsApproved, dto.RejectReason);
                return Ok(new { success = result });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Librarian/Admin: xác nhận trả sách
        [HttpPut("return/{id}")]
        [Authorize(Roles = "Librarian, Admin")]
        public async Task<IActionResult> ConfirmReturn(int id)
        {
            try
            {
                var result = await _borrowService.ConfirmReturnAsync(id);
                return Ok(new { success = result });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Librarian/Admin: lấy danh sách sách quá hạn
        [HttpGet("overdue")]
        [Authorize(Roles = "Librarian, Admin")]
        public async Task<IActionResult> GetOverdueRequests()
        {
            var requests = await _borrowService.GetOverdueRequestsAsync();
            return Ok(requests);
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int id))
                return id;
            return null;
        }
        [HttpGet("borrowed")]
        [Authorize(Roles = "Librarian, Admin")]
        public async Task<IActionResult> GetBorrowedRequests()
        {
            return Ok(await _borrowService.GetBorrowedRequestsAsync());
        }
    }
}