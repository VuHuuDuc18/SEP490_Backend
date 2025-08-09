using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.Request.LivestockCircle
{
    public class ReleaseBarnRequest
    {
        public Guid LivestockCircleId { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}
