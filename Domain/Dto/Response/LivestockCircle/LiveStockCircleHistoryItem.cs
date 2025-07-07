using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.LivestockCircle
{
    public class LiveStockCircleHistoryItem
    {
        public Guid Id { get; set; }
        public string LivestockCircleName { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalUnit { get; set; }
        public int DeadUnit { get; set; }
        public float AverageWeight { get; set; }
        public Guid BreedId { get; set; }
        public string BreedName { get; set; }
        
    }


}
