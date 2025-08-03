using Domain.Dto.Response.Barn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.Bill
{
    public class BillResponse
    {
        public Guid Id { get; set; }
        public UserRequestResponse UserRequest { get; set; }
        public LivestockCircleBillResponse LivestockCircle { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public string TypeBill { get; set; }
        public int Total { get; set; }
        public float? Weight { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<BillItemResponse> BillItem { get; set; }
    }

    public class UserRequestResponse
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }

    public class LivestockCircleBillResponse
    {
        public Guid Id { get; set; }
        public string LivestockCircleName { get; set; }
        public BarnDetailResponse BarnDetailResponse { get; set; }
    }

    public class BarnDetailResponse
    {
        public Guid Id { get; set; }
        public string BarnName { get; set; }
        public string Address { get; set; }
        public string Image { get; set; }
        public WokerResponse Worker { get; set; }
    }
}
