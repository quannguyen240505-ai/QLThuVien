using APIQLTV.DTOs.Auth;
using APIQLTV.Models;
using APIQLTV.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace APIQLTV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly IMemoryCache _cache;

        public AuthController(AppDbContext context, IConfiguration configuration, EmailService emailService, IMemoryCache cache)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _cache = cache;
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
            if (!user.IsActive)
            {
                return BadRequest("Tài khoản đã bị khóa.");
            }

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

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action(
                nameof(GoogleCallback),
                "Auth",
                null,
                Request.Scheme
            );

            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var frontendSuccessUrl = _configuration["GoogleAuth:FrontendSuccessUrl"];

            if (string.IsNullOrWhiteSpace(frontendSuccessUrl))
            {
                return BadRequest("Chưa cấu hình FrontendSuccessUrl.");
            }


            var result = await HttpContext.AuthenticateAsync("ExternalCookie");

            if (!result.Succeeded || result.Principal == null)
            {
                return Redirect($"{frontendSuccessUrl}?error=google_auth_failed");
            }

            var gmail = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(gmail))
            {
                return Redirect($"{frontendSuccessUrl}?error=email_not_found");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Gmail == gmail);

            if (user == null)
            {
                user = new AppUser
                {
                    Username = await GenerateUniqueUsernameFromEmail(gmail),
                    Gmail = gmail,
                    DateOfBirth = DateTime.Today,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    Role = "Member",
                    CreatedAt = DateTime.Now,
                    AuthProvider="google"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            if (!user.IsActive)
            {
                return Redirect($"{frontendSuccessUrl}?error=account_locked");
            }

            var tokenResult = CreateToken(user);

            var authResponse = new AuthResponse
            {
                Token = tokenResult.Token,
                Username = user.Username,
                Gmail = user.Gmail,
                DateOfBirth = user.DateOfBirth,
                Role = user.Role,
                Expiration = tokenResult.Expiration
            };

            var temporaryCode = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "");

            _cache.Set(
                $"social_login_{temporaryCode}",
                authResponse,
                TimeSpan.FromMinutes(2)
            );

            await HttpContext.SignOutAsync("ExternalCookie");

            return Redirect($"{frontendSuccessUrl}?code={temporaryCode}");
        }

        [HttpPost("exchange-social-code")]
        public IActionResult ExchangeSocialCode(ExchangeSocialCodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Mã đăng nhập không hợp lệ.");
            }

            var cacheKey = $"social_login_{request.Code}";

            if (!_cache.TryGetValue(cacheKey, out AuthResponse? authResponse) || authResponse == null)
            {
                return BadRequest("Mã đăng nhập đã hết hạn hoặc không hợp lệ.");
            }

            _cache.Remove(cacheKey);

            return Ok(authResponse);
        }
        private async Task<string> GenerateUniqueUsernameFromEmail(string email)
        {
            var baseUsername = email.Split('@')[0];
            var username = baseUsername;
            var count = 1;

            while (await _context.Users.AnyAsync(u => u.Username == username))
            {
                username = $"{baseUsername}{count}";
                count++;
            }

            return username;
        }
    }

}
