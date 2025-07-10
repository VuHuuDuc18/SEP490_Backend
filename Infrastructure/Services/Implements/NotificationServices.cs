using Domain.IServices;
using Entities.EntityModel;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public  class NotificationServices : INotificationService
    {
        private readonly IRepository<Notification> _notificationRepository;
        public NotificationServices() { }
    }
}
