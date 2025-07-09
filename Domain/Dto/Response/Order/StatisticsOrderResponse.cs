using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.Order
{


    public class StatisticsOrderResponse
    {
        public List<OrderItem> items;
        public float TotalRevenue { get; set; }
        
    }

    public class OrderItem
    {
        public Guid? BreedId { get; set; }
        public string? BreedName {  get; set; }
        public string? BreedCategoryName { get; set; }
        public int? TotalGoodUnitStockSold { get; set; }
        public int? TotalBadUnitStockSold { get; set; }
        public float? Revenue { get; set; }
        //public string Status { get; set; }
    }
}
