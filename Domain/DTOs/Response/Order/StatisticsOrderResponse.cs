using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.Response.Order
{
    public class StatisticsOrderResponse
    {
        
        public float TotalRevenue { get; set; }
        public int TotalGoodUnitStockSold { get; set; }
        public int TotalBadUnitStockSold { get; set; }
        [JsonProperty("datas")]
        public List<OrderItem>? datas;

    }

    public class OrderItem
    {
        public Guid? BreedId { get; set; }
        public string? BreedName { get; set; }
        public string? BreedCategoryName { get; set; }
        public int? GoodUnitStockSold { get; set; }
        public int? BadUnitStockSold { get; set; }
        public float? AverageGoodUnitPrice { get; set; }
        public float? AverageBadUnitPrice { get; set; }
        public float? Revenue { get; set; }
        //public string Status { get; set; }
    }
}
