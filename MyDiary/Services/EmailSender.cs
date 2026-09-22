using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendVerificationCodeAsync(string toEmail, string code)
        {
            // Always output to console for easy testing & local development
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n=======================================================");
            Console.WriteLine($"[EMAIL VERIFICATION OTP] To: {toEmail} | Code: {code} (Valid for 60s)");
            Console.WriteLine($"=======================================================\n");
            Console.ResetColor();

            _logger.LogInformation("Email verification OTP for {Email}: {Code} (Valid for 60s)", toEmail, code);

            var htmlBody = $@"
            <div style=""font-family: 'Segoe UI', Helvetica, Arial, sans-serif; max-width: 520px; margin: 0 auto; background: #f8fafc; padding: 24px; border-radius: 12px; border: 1px solid #e2e8f0;"">
                <div style=""text-align: center; margin-bottom: 20px;"">
                    <span style=""font-size: 32px;"">📔</span>
                    <h2 style=""color: #2563eb; margin: 8px 0 0; font-size: 22px; font-weight: 800;"">My Diary</h2>
                    <p style=""color: #64748b; font-size: 14px; margin: 4px 0 0;"">Lưu giữ từng khoảnh khắc cuộc sống</p>
                </div>
                <div style=""background: #ffffff; padding: 24px; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.06); text-align: center;"">
                    <h3 style=""color: #0f172a; margin: 0 0 12px; font-size: 17px;"">Mã xác thực tài khoản của bạn</h3>
                    <p style=""color: #475569; font-size: 14px; margin: 0 0 20px; line-height: 1.5;"">
                        Bạn vừa yêu cầu đăng ký tài khoản tại <strong>My Diary</strong>. Vui lòng nhập mã xác thực dưới đây để hoàn tất:
                    </p>
                    <div style=""background: #eff6ff; border: 2px dashed #93c5fd; border-radius: 8px; padding: 14px 20px; display: inline-block; margin: 0 auto 16px;"">
                        <span style=""font-size: 30px; font-weight: 800; letter-spacing: 8px; color: #1d4ed8;"">{code}</span>
                    </div>
                    <p style=""color: #dc2626; font-size: 13px; font-weight: 600; margin: 0 0 16px;"">
                        ⏱️ Mã này chỉ có hiệu lực trong vòng 60 giây.
                    </p>
                    <p style=""color: #94a3b8; font-size: 12px; margin: 0; line-height: 1.4;"">
                        Nếu bạn không thực hiện yêu cầu này, xin vui lòng bỏ qua email. Tuyệt đối không chia sẻ mã này cho bất kỳ ai.
                    </p>
                </div>
            </div>";

            // 1. Try Resend HTTP API first (Works over port 443 HTTPS - Never blocked on Render Free)
            var resendApiKey = (_configuration["Resend:ApiKey"]
                ?? Environment.GetEnvironmentVariable("RESEND_API_KEY")
                ?? Environment.GetEnvironmentVariable("Resend__ApiKey"))?.Trim();
            var resendFrom = _configuration["Resend:From"]?.Trim()
                ?? Environment.GetEnvironmentVariable("RESEND_FROM")
                ?? "My Diary <onboarding@resend.dev>";

            if (!string.IsNullOrWhiteSpace(resendApiKey))
            {
                try
                {
                    using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(6) };
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", resendApiKey);

                    var resendPayload = new
                    {
                        from = resendFrom,
                        to = new[] { toEmail },
                        subject = $"[My Diary] Mã xác thực đăng ký tài khoản: {code}",
                        html = htmlBody
                    };

                    var content = new StringContent(JsonSerializer.Serialize(resendPayload), Encoding.UTF8, "application/json");
                    var resendResponse = await httpClient.PostAsync("https://api.resend.com/emails", content);

                    if (resendResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Verification email sent successfully via Resend HTTPS API to {Email}", toEmail);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[RESEND API SUCCESS] Sent verification email to {toEmail}");
                        Console.ResetColor();
                        return;
                    }
                    else
                    {
                        var resendErr = await resendResponse.Content.ReadAsStringAsync();
                        _logger.LogWarning("Resend API returned {StatusCode}: {Error}. Falling back to MailKit.", resendResponse.StatusCode, resendErr);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[RESEND NOTICE] {resendResponse.StatusCode}: {resendErr}");
                        Console.ResetColor();
                    }
                }
                catch (Exception resendEx)
                {
                    _logger.LogWarning(resendEx, "Resend API call failed: {Message}. Falling back to MailKit.", resendEx.Message);
                }
            }

            // 2. Fallback to MailKit SMTP
            var host = _configuration["Smtp:Host"];
            var portStr = _configuration["Smtp:Port"];
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@mydiary.local";
            var fromName = _configuration["Smtp:FromName"] ?? "My Diary";

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("SMTP is not configured in appsettings.json. Verification code was logged to console.");
                return;
            }

            int port = 587;
            if (!string.IsNullOrEmpty(portStr) && int.TryParse(portStr, out int parsedPort))
            {
                port = parsedPort;
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress(toEmail, toEmail));
                message.Subject = $"[My Diary] Mã xác thực đăng ký tài khoản: {code}";

                var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                client.Timeout = 5000; // 5 seconds max timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                var socketOption = (port == 465) ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                await client.ConnectAsync(host, port, socketOption, cts.Token);
                await client.AuthenticateAsync(username.Trim(), password.Trim(), cts.Token);
                await client.SendAsync(message, cts.Token);
                await client.DisconnectAsync(true, cts.Token);

                _logger.LogInformation("Verification email sent successfully via MailKit to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[EMAIL SEND FAILED] To: {toEmail} | Error: {ex.GetType().Name}: {ex.Message}\n");
                Console.ResetColor();
                _logger.LogError(ex, "Failed to send email via MailKit to {Email}. Reason: {Message}", toEmail, ex.Message);
            }
        }
    }
}
