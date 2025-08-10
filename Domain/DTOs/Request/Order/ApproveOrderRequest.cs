using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.Request.Order
{
    public class ApproveOrderRequest
    {
        public Guid OrderId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Giá trị phải là số dương lớn hơn 0")]
        public float GoodUnitPrice { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Giá trị phải là số dương")]
        public float BadUnitPrice { get; set; }
        public bool IsDone { get; set; } = false;
        public int? GoodUnitStock { get; set; }
        public int? BadUnitStock { get; set; }
    }
}
