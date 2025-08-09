namespace manyasligida.Services
{
    public interface IEmailService
    {
        Task<bool> SendVerificationEmailAsync(string email, string verificationCode);
        Task<bool> SendEmailVerificationAsync(string email, string verificationCode);
        Task<bool> SendPasswordResetEmailAsync(string email, string resetCode);
    }
}
