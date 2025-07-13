using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Domain.IServices;
using Entities.EntityModel;

namespace SEP490_BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService){
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications(Guid userId){
            var result = await _notificationService.GetNotifications(userId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotification(Guid id){
            var result = await _notificationService.GetNotification(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotification(Notification notification){
            var result = await _notificationService.CreateNotification(notification);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotification(Guid id, Notification notification){
            var result = await _notificationService.UpdateNotification(notification);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id){
            var result = await _notificationService.DeleteNotification(id);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllNotifications(Guid userId){
            var result = await _notificationService.DeleteAllNotifications(userId);
            return Ok(result);
        }

        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(Guid id){
            var result = await _notificationService.MarkAsRead(id);
            return Ok(result);
        }

        [HttpPut("{id}/mark-as-unread")]
        public async Task<IActionResult> MarkAsUnread(Guid id){
            var result = await _notificationService.MarkAsUnread(id);
            return Ok(result);
        }


    }
}
