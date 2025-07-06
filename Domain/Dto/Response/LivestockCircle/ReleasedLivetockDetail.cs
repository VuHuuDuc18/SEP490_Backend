using Domain.Dto.Response.Barn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.LivestockCircle
{
    public class ReleasedLivetockDetail
    {
        public Guid LivestockCircleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalUnit { get; set; }
        public float AverageWeight { get; set; }
        public int GoodUnitNumber { get; set; }
        public int BadUnitNumber { get; set; }
        public string BreedName { get; set; }
        public string BreedCategoryName { get; set; }
        public BarnResponse BarnDetail {  get; set; }
        public List<string> ImageLinks { get; set; } = new List<string>();

    }
    public class ReleasedLivetockItem
    {
        public Guid LivestockCircleId { get; set; }
        public int TotalUnit { get; set; }
        public string BreedName { get; set; }
        public string BreedCategoryName { get; set; }
        public string BarnName { get; set; }

    }


}
