using Domain.IServices;
using Entities.EntityModel;
using Infrastructure.Repository;
using Application.Wrappers;
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

        public Task<Response<Notification>> CreateNotification(Notification notification)
        {
            throw new NotImplementedException();
        }

        public Task<Response<Notification>> DeleteAllNotifications(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<Response<Notification>> DeleteNotification(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Response<Notification>> GetNotification(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Response<List<Notification>>> GetNotifications(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<Response<Notification>> MarkAsRead(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Response<Notification>> MarkAsUnread(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Response<Notification>> UpdateNotification(Notification notification)
        {
            throw new NotImplementedException();
        }
    }
}
