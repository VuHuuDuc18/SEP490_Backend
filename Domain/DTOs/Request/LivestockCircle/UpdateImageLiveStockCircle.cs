using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.Request.LivestockCircle
{
    public class UpdateImageLiveStockCircle
    {
        public List<string> Images { get; set; }  = new List<string>();
    }
}
