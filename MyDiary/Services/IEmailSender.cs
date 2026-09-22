using System.Threading.Tasks;

namespace Services
{
    public interface IEmailSender
    {
        Task SendVerificationCodeAsync(string toEmail, string code);
    }
}
