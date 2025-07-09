using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.Order
{
    public class StatisticsOrderRequest
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        [Range(1, 4)]
        public int? Quater { get; set; }
        [Range(1, 12)]
        public int? Month { get; set; }
        public int Year { get; set; }
    }
}
