using System.Net;
using System.Net.Mail;

namespace APIQLTV.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendResetPasswordPinAsync(string toEmail, string pin)
        {
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"];
            var appPassword = _configuration["EmailSettings:AppPassword"];

            var subject = "Mã PIN đặt lại mật khẩu - QNU Library";

            var body = $@"
                <h2>QNU Library</h2>
                <p>Bạn vừa yêu cầu đặt lại mật khẩu.</p>
                <p>Mã PIN của bạn là:</p>
                <h1 style='letter-spacing:4px;color:#2563eb;'>{pin}</h1>
                <p>Mã PIN có hiệu lực trong 5 phút.</p>
                <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
            ";

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(senderEmail, appPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail!, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await smtp.SendMailAsync(message);
        }
    }
}
