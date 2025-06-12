namespace Infrastructure.Services
{
    public interface IEmailService
    {
        public Task SendAsync(string Email);
    }
}