using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.Bill
{
    public class CreateRequestDto
    {
        public Guid LivestockCircleId { get; set; } // ID của LivestockCircle
        public Guid ItemId { get; set; } // ID của item (Food, Medicine, hoặc Breed)
        public int Quantity { get; set; } // Số lượng yêu cầu
        public string Note { get; set; } // Ghi chú cho yêu cầu
        public string ItemType { get; set; } // Loại item (Food, Medicine, hoặc Breed)
    }
}
