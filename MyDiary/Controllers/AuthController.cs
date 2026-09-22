using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DBConnect;
using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;

        public AuthController(
            UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _configuration = configuration;
            _db = db;
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

            var existingUser = await _userManager.FindByNameAsync(dto.Username);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Tên tài khoản này đã được sử dụng. Vui lòng chọn tên khác." });
            }

            var user = new IdentityUser
            {
                UserName = dto.Username,
                Email = $"{dto.Username}@mydiary.local"
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

            var authResponse = GenerateJwtToken(user);
            return Ok(authResponse);
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

            var user = await _userManager.FindByNameAsync(dto.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác." });
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
                Email = user.Email
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
                ExpiresAt = expiresAt
            };
        }
    }
}
