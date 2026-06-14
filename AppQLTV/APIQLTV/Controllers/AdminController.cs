using APIQLTV.DTOs.Admin;
using APIQLTV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIQLTV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Admin/users
        [HttpGet("users")]
        public async Task<ActionResult<List<UserResponse>>> GetUsers()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Username = u.Username,
                    Gmail = u.Gmail,
                    DateOfBirth = u.DateOfBirth,
                    Role = u.Role,
                    AuthProvider = u.AuthProvider,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/Admin/users/1
        [HttpGet("users/{id}")]
        public async Task<ActionResult<UserResponse>> GetUserById(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Username = u.Username,
                    Gmail = u.Gmail,
                    DateOfBirth = u.DateOfBirth,
                    Role = u.Role,
                    AuthProvider = u.AuthProvider,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            return Ok(user);
        }

        // POST: api/Admin/users
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser(CreateUserRequest request)
        {
            if (!IsValidRole(request.Role))
            {
                return BadRequest("Role không hợp lệ.");
            }

            bool usernameExists = await _context.Users
                .AnyAsync(u => u.Username == request.Username);

            if (usernameExists)
            {
                return BadRequest("Tên đăng nhập đã tồn tại.");
            }

            bool gmailExists = await _context.Users
                .AnyAsync(u => u.Gmail == request.Gmail);

            if (gmailExists)
            {
                return BadRequest("Gmail đã được sử dụng.");
            }

            var user = new AppUser
            {
                Username = request.Username,
                Gmail = request.Gmail,
                DateOfBirth = request.DateOfBirth,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                AuthProvider = "Local",
                IsActive = request.IsActive,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Tạo tài khoản thành công.");
        }

        // PUT: api/Admin/users/1
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
        {
            if (!IsValidRole(request.Role))
            {
                return BadRequest("Role không hợp lệ.");
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            bool usernameExists = await _context.Users
                .AnyAsync(u => u.Username == request.Username && u.Id != id);

            if (usernameExists)
            {
                return BadRequest("Tên đăng nhập đã tồn tại.");
            }

            bool gmailExists = await _context.Users
                .AnyAsync(u => u.Gmail == request.Gmail && u.Id != id);

            if (gmailExists)
            {
                return BadRequest("Gmail đã được sử dụng.");
            }

            user.Username = request.Username;
            user.Gmail = request.Gmail;
            user.DateOfBirth = request.DateOfBirth;
            user.Role = request.Role;
            user.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return Ok("Cập nhật tài khoản thành công.");
        }

        // PUT: api/Admin/users/1/role
        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, UpdateUserRoleRequest request)
        {
            if (!IsValidRole(request.Role))
            {
                return BadRequest("Role không hợp lệ.");
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            user.Role = request.Role;

            await _context.SaveChangesAsync();

            return Ok("Cập nhật quyền thành công.");
        }

        // PUT: api/Admin/users/1/status
        [HttpPut("users/{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, UpdateUserStatusRequest request)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            user.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return Ok(request.IsActive ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.");
        }

        // PUT: api/Admin/users/1/password
        [HttpPut("users/{id}/password")]
        public async Task<IActionResult> ResetUserPassword(int id, AdminResetUserPasswordRequest request)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }
            if (user.AuthProvider == "Google")
            {
                return BadRequest("Không thể reset mật khẩu cho tài khoản Google.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.AuthProvider = "Local";

            await _context.SaveChangesAsync();

            return Ok("Đặt lại mật khẩu thành công.");
        }

        // DELETE: api/Admin/users/1
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("Xóa tài khoản thành công.");
        }

        // GET: api/Admin/statistics
        [HttpGet("statistics")]
        public async Task<ActionResult<AdminStatisticsResponse>> GetStatistics()
        {
            var statistics = new AdminStatisticsResponse
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalMembers = await _context.Users.CountAsync(u => u.Role == "Member"),
                TotalLibrarians = await _context.Users.CountAsync(u => u.Role == "Librarian"),
                TotalAdmins = await _context.Users.CountAsync(u => u.Role == "Admin"),
                TotalActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                TotalLockedUsers = await _context.Users.CountAsync(u => !u.IsActive),
                TotalLocalAccounts = await _context.Users.CountAsync(u => u.AuthProvider == "Local"),
                TotalGoogleAccounts = await _context.Users.CountAsync(u => u.AuthProvider == "Google")
            };

            return Ok(statistics);
        }

        private bool IsValidRole(string role)
        {
            return role == "Member" || role == "Librarian" || role == "Admin";
        }
    }
}