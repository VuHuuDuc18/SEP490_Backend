using Application.Wrappers;
using Entities.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.IServices
{
    public interface INotificationService
    {
        Task<Response<List<Notification>>> GetNotifications(Guid userId);
        Task<Response<Notification>> GetNotification(Guid id);
        Task<Response<Notification>> CreateNotification(Notification notification);
        Task<Response<Notification>> UpdateNotification(Notification notification);
        Task<Response<Notification>> MarkAsRead(Guid id);
        Task<Response<Notification>> MarkAsUnread(Guid id);
        Task<Response<Notification>> DeleteNotification(Guid id);
        Task<Response<Notification>> DeleteAllNotifications(Guid userId);
    }
}
