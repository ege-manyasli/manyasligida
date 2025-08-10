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
                var subject = "E-posta Doğrulama Kodu - Manyaslı Süt Ürünleri";
                var body = GenerateVerificationEmailBody(verificationCode);
                
                Console.WriteLine($"📧 Doğrulama e-postası gönderiliyor: {email}");
                var result = await SendEmailAsync(email, subject, body);
                
                if (result)
                {
                    Console.WriteLine($"✅ Doğrulama e-postası başarıyla gönderildi: {email}");
                }
                else
                {
                    Console.WriteLine($"❌ Doğrulama e-postası gönderilemedi: {email}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Doğrulama e-postası hatası: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendEmailVerificationAsync(string email, string verificationCode)
        {
            // Alias for SendVerificationEmailAsync
            return await SendVerificationEmailAsync(email, verificationCode);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetCode)
        {
            try
            {
                var subject = "Şifre Sıfırlama Kodu - Manyaslı Süt Ürünleri";
                var body = GeneratePasswordResetEmailBody(resetCode);
                
                Console.WriteLine($"📧 Şifre sıfırlama e-postası gönderiliyor: {email}");
                var result = await SendEmailAsync(email, subject, body);
                
                if (result)
                {
                    Console.WriteLine($"✅ Şifre sıfırlama e-postası başarıyla gönderildi: {email}");
                }
                else
                {
                    Console.WriteLine($"❌ Şifre sıfırlama e-postası gönderilemedi: {email}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Şifre sıfırlama e-postası hatası: {ex.Message}");
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

                // Configuration kontrolü
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) || 
                    string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(fromEmail))
                {
                    Console.WriteLine("❌ Email ayarları eksik veya hatalı");
                    return false;
                }

                Console.WriteLine($"📧 Email gönderiliyor...");
                Console.WriteLine($"To: {to}");
                Console.WriteLine($"From: {fromEmail}");
                Console.WriteLine($"SMTP: {smtpServer}:{smtpPort}");

                // Gmail SMTP ayarları - Basit konfigürasyon (üye olurken çalışan yapı)
                using var smtpClient = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    Timeout = 30000
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                
                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);
                
                Console.WriteLine($"✅ Email başarıyla gönderildi: {to}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Email gönderimi başarısız: {to}");
                Console.WriteLine($"Hata: {ex.Message}");
                
                // Development ortamında başarılı say
                var environment = _configuration["ASPNETCORE_ENVIRONMENT"];
                if (environment == "Development")
                {
                    Console.WriteLine("🔄 Development ortamında - e-posta başarılı sayılıyor");
                    return true;
                }
                
                return false;
            }
        }



        private string GenerateVerificationEmailBody(string verificationCode)
        {
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://manyaslisuturunleri.com";
            
            var sb = new StringBuilder();
            sb.AppendLine("<html><body style='font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f8f9fa;'>");
            sb.AppendLine("<div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.1);'>");
            
            // Header with gradient background
            sb.AppendLine("<div style='background: linear-gradient(135deg, #28a745 0%, #20c997 100%); padding: 30px 20px; text-align: center;'>");
            sb.AppendLine("<img src='https://manyaslisuturunleri.com/img/manyasli-gida.png' alt='Manyaslı Süt Ürünleri' style='max-height: 60px; margin-bottom: 15px;'>");
            sb.AppendLine("<h1 style='color: white; margin: 0; font-size: 28px; font-weight: bold;'>Manyaslı Süt Ürünleri</h1>");
            sb.AppendLine("<p style='color: rgba(255,255,255,0.9); margin: 8px 0 0; font-size: 16px;'>Kaliteli ve Taze Ürünler</p>");
            sb.AppendLine("</div>");
            
            // Content area
            sb.AppendLine("<div style='padding: 40px 30px;'>");
            sb.AppendLine("<div style='text-align: center; margin-bottom: 30px;'>");
            sb.AppendLine("<h2 style='color: #2c3e50; margin: 0 0 10px; font-size: 24px; font-weight: bold;'>E-posta Doğrulama</h2>");
            sb.AppendLine("<p style='color: #7f8c8d; margin: 0; font-size: 16px;'>Hesabınızı doğrulamak için aşağıdaki kodu kullanın</p>");
            sb.AppendLine("</div>");
            
            // Verification code box
            sb.AppendLine($"<div style='background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%); border: 2px solid #28a745; border-radius: 12px; padding: 25px; text-align: center; margin: 30px 0;'>");
            sb.AppendLine($"<div style='background-color: white; border-radius: 8px; padding: 20px; display: inline-block; min-width: 200px;'>");
            sb.AppendLine($"<h1 style='color: #28a745; font-size: 36px; letter-spacing: 8px; margin: 0; font-weight: bold; font-family: monospace;'>{verificationCode}</h1>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            
            // Instructions
            sb.AppendLine("<div style='background-color: #f8f9fa; border-radius: 8px; padding: 20px; margin: 30px 0;'>");
            sb.AppendLine("<h3 style='color: #495057; margin: 0 0 15px; font-size: 18px;'>Önemli Bilgiler:</h3>");
            sb.AppendLine("<ul style='color: #6c757d; margin: 0; padding-left: 20px;'>");
            sb.AppendLine("<li style='margin-bottom: 8px;'>Bu kod 15 dakika geçerlidir</li>");
            sb.AppendLine("<li style='margin-bottom: 8px;'>Kodu kimseyle paylaşmayın</li>");
            sb.AppendLine("<li style='margin-bottom: 0;'>Bu e-postayı siz talep etmediyseniz, lütfen dikkate almayın</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("</div>");
            
            // Verify button
            sb.AppendLine($"<div style='text-align: center; margin: 40px 0;'>");
            sb.AppendLine($"<a href='{baseUrl}/Account/VerifyEmail' style='background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 18px 40px; text-decoration: none; border-radius: 50px; font-weight: bold; font-size: 18px; display: inline-block; box-shadow: 0 6px 20px rgba(40, 167, 69, 0.4); transition: all 0.3s ease;'>");
            sb.AppendLine("🔐 E-postamı Doğrula");
            sb.AppendLine("</a>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("</div>");
            
            // Footer
            sb.AppendLine("<div style='background-color: #343a40; color: #ffffff; text-align: center; padding: 20px;'>");
            sb.AppendLine("<p style='margin: 0; font-size: 14px;'>© 2024 Manyaslı Süt Ürünleri. Tüm hakları saklıdır.</p>");
            sb.AppendLine("<p style='margin: 5px 0 0; font-size: 12px; color: #adb5bd;'>Bu e-posta otomatik olarak gönderilmiştir.</p>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("</div></body></html>");
            
            return sb.ToString();
        }

        private string GeneratePasswordResetEmailBody(string resetCode)
        {
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://manyaslisuturunleri.com";
            var verifyUrl = $"{baseUrl}/Account/VerifyResetCode";
            
            var sb = new StringBuilder();
            sb.AppendLine("<html><body style='font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f8f9fa;'>");
            sb.AppendLine("<div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.1);'>");
            
            // Header with gradient background
            sb.AppendLine("<div style='background: linear-gradient(135deg, #28a745 0%, #20c997 100%); padding: 30px 20px; text-align: center;'>");
            sb.AppendLine("<img src='https://manyaslisuturunleri.com/img/manyasli-gida.png' alt='Manyaslı Süt Ürünleri' style='max-height: 60px; margin-bottom: 15px;'>");
            sb.AppendLine("<h1 style='color: white; margin: 0; font-size: 28px; font-weight: bold;'>Manyaslı Süt Ürünleri</h1>");
            sb.AppendLine("<p style='color: rgba(255,255,255,0.9); margin: 8px 0 0; font-size: 16px;'>Kaliteli ve Taze Ürünler</p>");
            sb.AppendLine("</div>");
            
            // Content area
            sb.AppendLine("<div style='padding: 40px 30px;'>");
            sb.AppendLine("<div style='text-align: center; margin-bottom: 30px;'>");
            sb.AppendLine("<h2 style='color: #2c3e50; margin: 0 0 10px; font-size: 24px; font-weight: bold;'>Şifre Sıfırlama Kodu</h2>");
            sb.AppendLine("<p style='color: #7f8c8d; margin: 0; font-size: 16px;'>Şifrenizi sıfırlamak için aşağıdaki kodu kullanın</p>");
            sb.AppendLine("</div>");
            
            // Verification code box
            sb.AppendLine($"<div style='background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%); border: 2px solid #28a745; border-radius: 12px; padding: 25px; text-align: center; margin: 30px 0;'>");
            sb.AppendLine($"<div style='background-color: white; border-radius: 8px; padding: 20px; display: inline-block; min-width: 200px;'>");
            sb.AppendLine($"<h1 style='color: #28a745; font-size: 36px; letter-spacing: 8px; margin: 0; font-weight: bold; font-family: monospace;'>{resetCode}</h1>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            
            // Instructions
            sb.AppendLine("<div style='background-color: #fff3cd; border: 1px solid #ffeaa7; border-radius: 8px; padding: 20px; margin: 30px 0;'>");
            sb.AppendLine("<h3 style='color: #856404; margin: 0 0 15px; font-size: 18px;'>⚠️ Önemli Güvenlik Bilgileri:</h3>");
            sb.AppendLine("<ul style='color: #856404; margin: 0; padding-left: 20px;'>");
            sb.AppendLine("<li style='margin-bottom: 8px;'>Bu kod 10 dakika geçerlidir</li>");
            sb.AppendLine("<li style='margin-bottom: 8px;'>Kodu kimseyle paylaşmayın</li>");
            sb.AppendLine("<li style='margin-bottom: 8px;'>Şifrenizi güçlü ve benzersiz yapın</li>");
            sb.AppendLine("<li style='margin-bottom: 0;'>Bu e-postayı siz talep etmediyseniz, lütfen dikkate almayın</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("</div>");
            
            // Next steps
            sb.AppendLine("<div style='background-color: #f8f9fa; border-radius: 8px; padding: 20px; margin: 30px 0;'>");
            sb.AppendLine("<h3 style='color: #495057; margin: 0 0 15px; font-size: 16px;'>Sonraki Adımlar:</h3>");
            sb.AppendLine("<ol style='color: #6c757d; margin: 0; padding-left: 20px;'>");
            sb.AppendLine("<li style='margin-bottom: 8px;'>Yukarıdaki kodu kopyalayın</li>");
            sb.AppendLine("<li style='margin-bottom: 8px;'>Aşağıdaki butona tıklayarak doğrulama sayfasına gidin</li>");
            sb.AppendLine("<li style='margin-bottom: 0;'>Kodu girerek şifrenizi değiştirin</li>");
            sb.AppendLine("</ol>");
            sb.AppendLine("</div>");
            
            // Verify button
            sb.AppendLine($"<div style='text-align: center; margin: 40px 0;'>");
            sb.AppendLine($"<a href='{verifyUrl}' style='background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 18px 40px; text-decoration: none; border-radius: 50px; font-weight: bold; font-size: 18px; display: inline-block; box-shadow: 0 6px 20px rgba(40, 167, 69, 0.4); transition: all 0.3s ease;'>");
            sb.AppendLine("🔐 Kodu Doğrula");
            sb.AppendLine("</a>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("</div>");
            
            // Footer
            sb.AppendLine("<div style='background-color: #343a40; color: #ffffff; text-align: center; padding: 20px;'>");
            sb.AppendLine("<p style='margin: 0; font-size: 14px;'>© 2024 Manyaslı Süt Ürünleri. Tüm hakları saklıdır.</p>");
            sb.AppendLine("<p style='margin: 5px 0 0; font-size: 12px; color: #adb5bd;'>Bu e-posta otomatik olarak gönderilmiştir.</p>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("</div></body></html>");
            
            return sb.ToString();
        }
    }
}
