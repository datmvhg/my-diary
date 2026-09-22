using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DBConnect;
using DTOs;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Services;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public AuthController(
            UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            ApplicationDbContext db,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _configuration = configuration;
            _db = db;
            _emailSender = emailSender;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(new { message = errors });
            }

            var normalizedEmail = dto.Email.ToLower().Trim();

            // Check username
            var existingUserByName = await _userManager.FindByNameAsync(dto.Username.Trim());
            if (existingUserByName != null)
            {
                return BadRequest(new { message = "Tên tài khoản này đã được sử dụng. Vui lòng chọn tên khác." });
            }

            // Check email
            var existingUserByEmail = await _userManager.FindByEmailAsync(normalizedEmail);
            if (existingUserByEmail != null)
            {
                return BadRequest(new { message = "Địa chỉ email này đã được sử dụng cho tài khoản khác." });
            }

            var user = new IdentityUser
            {
                UserName = dto.Username.Trim(),
                Email = normalizedEmail,
                EmailConfirmed = false // Requires 60s verification code
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = errors });
            }

            // If there are legacy moments created before user authentication existed,
            // assign them to this first registered account
            var unassignedMoments = await _db.DiaryMoments.Where(m => m.UserId == null).ToListAsync();
            if (unassignedMoments.Any())
            {
                foreach (var moment in unassignedMoments)
                {
                    moment.UserId = user.Id;
                }
                await _db.SaveChangesAsync();
            }

            // Generate 6-digit OTP code valid for 60 seconds
            var code = GenerateRandom6DigitCode();
            var verificationRecord = new EmailVerificationCode
            {
                Email = normalizedEmail,
                Code = code,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddSeconds(60),
                IsUsed = false
            };

            _db.EmailVerificationCodes.Add(verificationRecord);
            await _db.SaveChangesAsync();

            // Send email using MailKit asynchronously in background so the HTTP request never hangs
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailSender.SendVerificationCodeAsync(user.Email ?? "", code);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EMAIL SEND ERROR] {ex.Message}");
                }
            });

            return Ok(new
            {
                message = "Đăng ký thành công. Mã xác thực 6 chữ số đã được gửi tới email của bạn (có hiệu lực trong 60 giây).",
                username = user.UserName,
                email = MaskEmail(user.Email),
                rawEmail = user.Email,
                expiresInSeconds = 60
            });
        }

        // POST: api/auth/send-verification-code
        // Resends a new verification code valid for 60 seconds
        [HttpPost("send-verification-code")]
        [AllowAnonymous]
        public async Task<IActionResult> SendVerificationCode([FromBody] SendVerificationCodeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Vui lòng cung cấp tài khoản hoặc email." });
            }

            var query = dto.UsernameOrEmail.Trim();
            var user = await _userManager.FindByNameAsync(query)
                       ?? await _userManager.FindByEmailAsync(query.ToLower());

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản tương ứng trong hệ thống." });
            }

            if (user.EmailConfirmed)
            {
                return BadRequest(new { message = "Tài khoản này đã được xác thực email trước đó." });
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                return BadRequest(new { message = "Tài khoản chưa cập nhật địa chỉ email." });
            }

            var normalizedEmail = user.Email.ToLower().Trim();

            // Invalidate existing unused codes for this email
            var oldCodes = await _db.EmailVerificationCodes
                .Where(c => c.Email == normalizedEmail && !c.IsUsed)
                .ToListAsync();
            foreach (var c in oldCodes)
            {
                c.IsUsed = true;
            }

            // Generate new 6-digit OTP valid for 60s
            var code = GenerateRandom6DigitCode();
            var newRecord = new EmailVerificationCode
            {
                Email = normalizedEmail,
                Code = code,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddSeconds(60),
                IsUsed = false
            };

            _db.EmailVerificationCodes.Add(newRecord);
            await _db.SaveChangesAsync();

            // Send via MailKit asynchronously in background so the HTTP request never hangs
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailSender.SendVerificationCodeAsync(user.Email ?? "", code);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EMAIL RESEND ERROR] {ex.Message}");
                }
            });

            return Ok(new
            {
                message = "Mã xác thực mới đã được gửi tới email của bạn (hiệu lực trong 60 giây).",
                email = MaskEmail(user.Email),
                rawEmail = user.Email,
                expiresInSeconds = 60
            });
        }

        // POST: api/auth/verify-email
        // Verifies the 6-digit code and activates the account
        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Vui lòng nhập đúng mã xác thực 6 chữ số." });
            }

            var query = dto.UsernameOrEmail.Trim();
            var user = await _userManager.FindByNameAsync(query)
                       ?? await _userManager.FindByEmailAsync(query.ToLower());

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản." });
            }

            if (user.EmailConfirmed)
            {
                var existingAuth = GenerateJwtToken(user);
                return Ok(new
                {
                    message = "Tài khoản đã được kích hoạt thành công.",
                    token = existingAuth.Token,
                    username = user.UserName,
                    userId = user.Id,
                    email = user.Email,
                    expiresAt = existingAuth.ExpiresAt
                });
            }

            var normalizedEmail = user.Email?.ToLower().Trim() ?? "";

            var record = await _db.EmailVerificationCodes
                .Where(c => c.Email == normalizedEmail && c.Code == dto.Code.Trim() && !c.IsUsed)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (record == null)
            {
                return BadRequest(new { message = "Mã xác thực không chính xác. Vui lòng kiểm tra lại." });
            }

            if (DateTime.UtcNow > record.ExpiresAt)
            {
                record.IsUsed = true;
                await _db.SaveChangesAsync();
                return BadRequest(new
                {
                    message = "Mã xác thực đã hết hạn (chỉ có hiệu lực trong 60 giây). Vui lòng bấm 'Gửi lại mã'.",
                    isExpired = true
                });
            }

            // Valid code: mark code as used and confirm user email
            record.IsUsed = true;
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            await _db.SaveChangesAsync();

            var authResponse = GenerateJwtToken(user);
            return Ok(new
            {
                message = "Xác thực email thành công! Đang chuyển hướng vào hệ thống...",
                token = authResponse.Token,
                username = user.UserName,
                userId = user.Id,
                email = user.Email,
                expiresAt = authResponse.ExpiresAt
            });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Vui lòng nhập đầy đủ tài khoản và mật khẩu." });
            }

            var query = dto.Username.Trim();
            var user = await _userManager.FindByNameAsync(query)
                       ?? await _userManager.FindByEmailAsync(query.ToLower());

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác." });
            }

            // If user has not verified their email, block login and require email verification
            if (!user.EmailConfirmed)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Tài khoản chưa được xác thực email. Vui lòng xác thực email để đăng nhập vào hệ thống.",
                    requiresEmailVerification = true,
                    username = user.UserName,
                    email = MaskEmail(user.Email),
                    rawEmail = user.Email
                });
            }

            var authResponse = GenerateJwtToken(user);
            return Ok(authResponse);
        }

        // GET: api/auth/me
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new UserProfileDto
            {
                UserId = user.Id,
                Username = user.UserName ?? "",
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed
            });
        }

        private AuthResponseDto GenerateJwtToken(IdentityUser user)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "MyDiarySuperSecretSecureKeyWithAtLeast32CharactersLong2026!";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "MyDiaryApi";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "MyDiaryClient";
            var expiresAt = DateTime.UtcNow.AddDays(7);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var stringToken = tokenHandler.WriteToken(token);

            return new AuthResponseDto
            {
                Token = stringToken,
                Username = user.UserName ?? "",
                UserId = user.Id,
                Email = user.Email,
                ExpiresAt = expiresAt
            };
        }

        private static string GenerateRandom6DigitCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private static string MaskEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) return email ?? "";
            var parts = email.Split('@');
            var name = parts[0];
            var domain = parts[1];
            if (name.Length <= 2) return $"{name[0]}*@{domain}";
            return $"{name.Substring(0, 2)}{new string('*', Math.Max(1, name.Length - 3))}{name[^1]}@{domain}";
        }
    }
}
