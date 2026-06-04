using APIQLTV.DTOs.Auth;
using APIQLTV.Models;
using APIQLTV.Services;
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
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        public AuthController(AppDbContext context, IConfiguration configuration, EmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest("Mật khẩu xác nhận không khớp.");
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
                Role = "Member",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
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

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return BadRequest("Sai tài khoản hoặc mật khẩu.");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash
            );

            if (!isPasswordValid)
            {
                return BadRequest("Sai tài khoản hoặc mật khẩu.");
            }

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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Gmail == request.Gmail);

            if (user == null)
            {
                return BadRequest("Gmail không tồn tại trong hệ thống.");
            }

            var random = new Random();
            string pin = random.Next(100000, 999999).ToString();

            user.ResetPasswordPin = pin;
            user.ResetPasswordPinExpires = DateTime.Now.AddMinutes(5);
            user.IsResetPasswordPinUsed = false;

            await _context.SaveChangesAsync();

            await _emailService.SendResetPasswordPinAsync(user.Gmail, pin);

            return Ok("Mã PIN đã được gửi về Gmail.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return BadRequest("Mật khẩu xác nhận không khớp.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Gmail == request.Gmail);

            if (user == null)
            {
                return BadRequest("Gmail không tồn tại trong hệ thống.");
            }

            if (user.IsResetPasswordPinUsed)
            {
                return BadRequest("Mã PIN đã được sử dụng.");
            }

            if (string.IsNullOrWhiteSpace(user.ResetPasswordPin))
            {
                return BadRequest("Bạn chưa yêu cầu mã PIN.");
            }

            if (user.ResetPasswordPinExpires == null || DateTime.Now > user.ResetPasswordPinExpires)
            {
                return BadRequest("Mã PIN đã hết hạn.");
            }

            if (user.ResetPasswordPin != request.Pin)
            {
                return BadRequest("Mã PIN không đúng.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            user.IsResetPasswordPinUsed = true;
            user.ResetPasswordPin = null;
            user.ResetPasswordPinExpires = null;

            await _context.SaveChangesAsync();

            return Ok("Đặt lại mật khẩu thành công.");
        }
    }
}
