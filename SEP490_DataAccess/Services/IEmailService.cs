namespace Infrastructure.Services
{
    public interface IEmailService
    {
        public Task SendEmailCreateAccountAsync(string Email, string Password);
    }
}