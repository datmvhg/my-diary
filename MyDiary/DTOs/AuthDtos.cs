using System;
using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên tài khoản")]
        [MinLength(3, ErrorMessage = "Tên tài khoản phải có ít nhất 3 ký tự")]
        [MaxLength(50, ErrorMessage = "Tên tài khoản không được vượt quá 50 ký tự")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận lại mật khẩu")]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên tài khoản hoặc email")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = string.Empty;
    }

    public class VerifyEmailDto
    {
        [Required(ErrorMessage = "Vui lòng cung cấp tài khoản hoặc email")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã xác thực OTP 6 số")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác thực phải gồm đúng 6 chữ số")]
        public string Code { get; set; } = string.Empty;
    }

    public class SendVerificationCodeDto
    {
        [Required(ErrorMessage = "Vui lòng cung cấp tài khoản hoặc email")]
        public string UsernameOrEmail { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class UserProfileDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
