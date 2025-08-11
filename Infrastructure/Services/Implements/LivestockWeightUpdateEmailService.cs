using Domain.Helper.Constants;
using Domain.IServices;
using Entities.EntityModel;
using Infrastructure.Extensions;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MimeKit;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class LivestockWeightUpdateEmailService : BackgroundService, ILivestockWeightUpdateEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _interval = TimeSpan.FromDays(1); // Kiểm tra mỗi ngày

        public LivestockWeightUpdateEmailService(IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckAndSendEmailsAsync();
                await Task.Delay(_interval, stoppingToken);
            }
        }

        public async Task CheckAndSendEmailsAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var livestockCircleRepository = scope.ServiceProvider.GetRequiredService<IRepository<LivestockCircle>>();
                var barnRepository = scope.ServiceProvider.GetRequiredService<IRepository<Barn>>();
                var userRepository = scope.ServiceProvider.GetRequiredService<IRepository<User>>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                // Lấy danh sách LivestockCircle thỏa mãn điều kiện
                var livestockCircles = await livestockCircleRepository.GetQueryable()
                    .Where(lc => lc.Status == StatusConstant.GROWINGSTAT && lc.StartDate.HasValue)
                    .ToListAsync();

                foreach (var circle in livestockCircles)
                {
                    var ageInDays = (DateTime.Now - circle.StartDate!.Value).Days;

                    if (ageInDays % 7 == 0 && ageInDays > 0)
                    {
                        var barn = await barnRepository.GetByIdAsync(circle.BarnId);
                        var worker = await userRepository.GetByIdAsync(barn.WorkerId);
                        var updateLink = "https://lcfms.sovasolutions.online/update-weight/" + circle.Id;
                        string body = MailBodyGenerate.BodyLivestockWeightUpdate(
                            workerName: worker?.FullName ?? "Nhân viên",
                            livestockCircleName: circle.LivestockCircleName,
                            barnName: barn?.BarnName ?? "Không xác định",
                            startDate: circle.StartDate!.Value,
                            ageInDays: ageInDays,
                            updateLink: updateLink
                        );
                        await emailService.SendEmailAsync(
                            worker?.Email,
                            EmailConstant.EMAILSUBJECTUPDATEWEIGHT,
                            body
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi kiểm tra và gửi email: {ex.Message}");
            }
        }
    }
}