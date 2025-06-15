using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class ImageDailyReport : EntityBase
    {
        public Guid DailyReportId { get; set; }
        public string Thumnail { get; set; }
        public string ImageLink { get; set; }
        public virtual DailyReport DailyReport { get; set; }
    }
}
