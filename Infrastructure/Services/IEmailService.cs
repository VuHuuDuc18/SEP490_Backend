namespace Infrastructure.Services
{
    public interface IEmailService
    {
        public Task SendEmailAsync(string Email, string Subject, string Body);
    }
}