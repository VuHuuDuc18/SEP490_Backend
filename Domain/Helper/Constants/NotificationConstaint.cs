namespace Domain.Helper.Constants
{
    public enum NotificationChannel
    {
        InApp = 1,
        Email = 2,
        Both = 3
    }
    
    public enum NotificationPriority
    {
        Low = 1,
        Medium = 2,
        High = 3
    }
    public enum NotificationType
    {
        Info = 1,
        Warning = 2,
        Error = 3,
        Success = 4,
        OrderUpdate = 5,
        BillApproved = 6,
        SystemMaintenance = 7
    }
    
}