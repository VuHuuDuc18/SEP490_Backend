using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Helper.Constants
{
    public static class StatusConstant
    {
        public readonly static string GROWINGSTAT = "GROWING";
        public readonly static string RELEASESTAT = "RELEASE";
        public readonly static string DONESTAT = "DONE";

        // Trạng thái cho luồng request
        public readonly static string REQUESTED = "REQUESTED"; // Yêu cầu đã được gửi
        public readonly static string APPROVED = "APPROVED";   // Yêu cầu đã được duyệt
        public readonly static string CONFIRMED = "CONFIRMED"; // Yêu cầu đã được xác nhận nhận
        public readonly static string REJECTED = "REJECTED";   // Yêu cầu bị từ chối
        public readonly static string COMPLETED = "COMPLETED"; // Yêu cầu hoàn tất (đã cập nhật kho)
        public readonly static string CANCELLED = "CANCELLED"; // Yêu cầu bị hủy
    }
}
