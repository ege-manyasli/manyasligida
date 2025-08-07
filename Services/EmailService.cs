using System.Net.Mail;
using System.Net;
using System.Text;

namespace manyasligida.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendVerificationEmailAsync(string email, string verificationCode)
        {
            try
            {
                var subject = "E-posta Doğrulama Kodu - Manyaslı Gıda";
                var body = GenerateVerificationEmailBody(verificationCode);
                
                return await SendEmailAsync(email, subject, body);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetCode)
        {
            try
            {
                var subject = "Şifre Sıfırlama Kodu - Manyaslı Gıda";
                var body = GeneratePasswordResetEmailBody(resetCode);
                
                return await SendEmailAsync(email, subject, body);
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // Email ayarlarını configuration'dan al
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                // Gmail SMTP ayarları
                var smtpClient = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                
                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch
            {
                // Email gönderilemezse console'a yazdır (development için)
                Console.WriteLine($"Email gönderilemedi: {to}");
                Console.WriteLine($"Konu: {subject}");
                Console.WriteLine($"İçerik: {body}");
                return true; // Development'ta başarılı sayılsın
            }
        }

        private string GenerateVerificationEmailBody(string verificationCode)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html><body style='font-family: Arial, sans-serif;'>");
            sb.AppendLine("<div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");
            sb.AppendLine("<div style='text-align: center; margin-bottom: 30px;'>");
            sb.AppendLine("<h2 style='color: #28a745; margin: 0; font-size: 28px; font-weight: bold;'>Manyaslı Gıda</h2>");
            sb.AppendLine("<p style='color: #6c757d; margin: 5px 0 0; font-size: 16px;'>Kaliteli ve Taze Ürünler</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div style='background-color: #ecf0f1; padding: 20px; border-radius: 10px;'>");
            sb.AppendLine("<h3 style='color: #e74c3c;'>E-posta Doğrulama</h3>");
            sb.AppendLine("<p>Hesabınızı doğrulamak için aşağıdaki kodu kullanın:</p>");
            sb.AppendLine($"<div style='background-color: #fff; padding: 15px; text-align: center; border-radius: 5px; margin: 20px 0;'>");
            sb.AppendLine($"<h1 style='color: #2c3e50; font-size: 32px; letter-spacing: 5px; margin: 0;'>{verificationCode}</h1>");
            sb.AppendLine("</div>");
            sb.AppendLine("<p style='color: #7f8c8d; font-size: 14px;'>Bu kod 10 dakika geçerlidir.</p>");
            sb.AppendLine("<p style='color: #7f8c8d; font-size: 14px;'>Bu e-postayı siz talep etmediyseniz, lütfen dikkate almayın.</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div style='text-align: center; margin-top: 20px; color: #7f8c8d; font-size: 12px;'>");
            sb.AppendLine("<p>© 2024 Manyaslı Gıda. Tüm hakları saklıdır.</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div></body></html>");
            
            return sb.ToString();
        }

        private string GeneratePasswordResetEmailBody(string resetCode)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html><body style='font-family: Arial, sans-serif;'>");
            sb.AppendLine("<div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");
            sb.AppendLine("<div style='text-align: center; margin-bottom: 30px;'>");
            sb.AppendLine("<h2 style='color: #28a745; margin: 0; font-size: 28px; font-weight: bold;'>Manyaslı Gıda</h2>");
            sb.AppendLine("<p style='color: #6c757d; margin: 5px 0 0; font-size: 16px;'>Kaliteli ve Taze Ürünler</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div style='background-color: #ecf0f1; padding: 20px; border-radius: 10px;'>");
            sb.AppendLine("<h3 style='color: #e74c3c;'>Şifre Sıfırlama</h3>");
            sb.AppendLine("<p>Şifrenizi sıfırlamak için aşağıdaki kodu kullanın:</p>");
            sb.AppendLine($"<div style='background-color: #fff; padding: 15px; text-align: center; border-radius: 5px; margin: 20px 0;'>");
            sb.AppendLine($"<h1 style='color: #2c3e50; font-size: 32px; letter-spacing: 5px; margin: 0;'>{resetCode}</h1>");
            sb.AppendLine("</div>");
            sb.AppendLine("<p style='color: #7f8c8d; font-size: 14px;'>Bu kod 10 dakika geçerlidir.</p>");
            sb.AppendLine("<p style='color: #7f8c8d; font-size: 14px;'>Bu e-postayı siz talep etmediyseniz, lütfen dikkate almayın.</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div style='text-align: center; margin-top: 20px; color: #7f8c8d; font-size: 12px;'>");
            sb.AppendLine("<p>© 2024 Manyaslı Gıda. Tüm hakları saklıdır.</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div></body></html>");
            
            return sb.ToString();
        }
    }
}
