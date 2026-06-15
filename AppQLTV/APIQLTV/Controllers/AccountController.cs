using APIQLTV.DTOs.Account;
using APIQLTV.DTOs.Auth;
using APIQLTV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace APIQLTV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/account/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized("Không tìm thấy thông tin người dùng.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            return Ok(new AccountProfileResponse
            {
                Id = user.Id,
                Username = user.Username,
                Gmail = user.Gmail,
                DateOfBirth = user.DateOfBirth,
                Role = user.Role,
                IsActive = user.IsActive,
                AuthProvider = user.AuthProvider ?? "Local",
                CreatedAt = user.CreatedAt
            });
        }

        // PUT: api/account/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized("Không tìm thấy thông tin người dùng.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            if (request.DateOfBirth.Date > DateTime.Now.Date)
            {
                return BadRequest("Ngày sinh không được lớn hơn ngày hiện tại.");
            }

            bool gmailExists = await _context.Users
                .AnyAsync(u => u.Gmail == request.Gmail && u.Id != user.Id);

            if (gmailExists)
            {
                return BadRequest("Gmail đã được sử dụng.");
            }

            user.Gmail = request.Gmail;
            user.DateOfBirth = request.DateOfBirth;

            await _context.SaveChangesAsync();

            var tokenResult = CreateToken(user);

            return Ok(new AuthResponse
            {
                Token = tokenResult.Token,
                Username = user.Username,
                Gmail = user.Gmail,
                DateOfBirth = user.DateOfBirth,
                Role = user.Role,
                Expiration = tokenResult.Expiration
            });
        }

        // PUT: api/account/change-password
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized("Không tìm thấy thông tin người dùng.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return BadRequest("Mật khẩu xác nhận không khớp.");
            }

            bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(
                request.CurrentPassword,
                user.PasswordHash
            );

            if (!isCurrentPasswordValid)
            {
                return BadRequest("Mật khẩu hiện tại không đúng.");
            }

            bool isSamePassword = BCrypt.Net.BCrypt.Verify(
                request.NewPassword,
                user.PasswordHash
            );

            if (isSamePassword)
            {
                return BadRequest("Mật khẩu mới không được trùng với mật khẩu hiện tại.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            await _context.SaveChangesAsync();

            return Ok("Đổi mật khẩu thành công.");
        }

        private int? GetCurrentUserId()
        {
            var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdText, out int userId))
            {
                return userId;
            }

            return null;
        }

        private TokenResult CreateToken(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Gmail),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("DateOfBirth", user.DateOfBirth.ToString("yyyy-MM-dd"))
            };

            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"]!);

            var expiration = DateTime.Now.AddMinutes(expireMinutes);

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey!)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            return new TokenResult
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }

        private class TokenResult
        {
            public string Token { get; set; } = string.Empty;

            public DateTime Expiration { get; set; }
        }
    }
}