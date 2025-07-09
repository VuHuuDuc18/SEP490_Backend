using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Helper.Constants
{
    public static class StatusConstant
    {
        // Livestock Circle status

        public readonly static string PENDINGSTAT = "PENDING";
        public readonly static string GROWINGSTAT = "GROWING";
        public readonly static string RELEASESTAT = "RELEASE";
        public readonly static string DONESTAT = "DONE";
        public readonly static string CANCELSTAT = "CANCEL"; 

        // Trạng thái cho luồng request
        // Bill Status

        public readonly static string REQUESTED = "REQUESTED"; // Yêu cầu đã được gửi
        public readonly static string APPROVED = "APPROVED";   // Yêu cầu đã được duyệt
        public readonly static string CONFIRMED = "CONFIRMED"; // Yêu cầu đã được xác nhận nhận
        public readonly static string REJECTED = "REJECTED";   // Yêu cầu bị từ chối
        public readonly static string COMPLETED = "COMPLETED"; // Yêu cầu hoàn tất (đã cập nhật kho)
        public readonly static string CANCELLED = "CANCELLED"; // Yêu cầu bị hủy


    }
    public static class OrderStatus
    {
        public readonly static string PENDING = "PENDING";
        public readonly static string APPROVED = "APPROVED";
        public readonly static string DENIED = "DENIED";
        public readonly static string CANCELLED = "CANCELLED";
    }

    public static class TypeBill 
    {
        public readonly static string FOOD = "Food";
        public readonly static string MEDICINE = "Medicine";
        public readonly static string BREED = "Breed";
    }

    public static class DailyReportStatus
    {
        public readonly static string TODAY = "TODAY";
        public readonly static string HISTORY = "HISTORY";
    }

}
