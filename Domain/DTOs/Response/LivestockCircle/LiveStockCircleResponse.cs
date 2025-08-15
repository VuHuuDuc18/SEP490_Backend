
﻿using Domain.Dto.Response.Barn;

﻿using Domain.Dto.Response.Bill;
using Domain.Dto.Response.User;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.LivestockCircle
{
    public class LivestockCircleResponse
    {
        public Guid Id { get; set; }
        public string LivestockCircleName { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int TotalUnit { get; set; }
        public int DeadUnit { get; set; }
        public float AverageWeight { get; set; }
        public int GoodUnitNumber { get; set; }
        public int BadUnitNumber { get; set; }
        public Guid BreedId { get; set; }
        public Guid BarnId { get; set; }
        public Guid TechicalStaffId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? PreSoldDate { get; set; } // set ngày bán dự kiến / sale set
        public float? SamplePrice { get; set; }
        public List<ImageLivestockCircleResponse>? Images { get; set; }
    }

    public class LiveStockCircleActive
    {
        public Guid Id { get; set; }
        public string LivestockCircleName { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public int TotalUnit { get; set; }
        public int DeadUnit { get; set; }
        public float AverageWeight { get; set; }
        public int GoodUnitNumber { get; set; }
        public int BadUnitNumber { get; set; }
        public BreedBillResponse Breed { get; set; }
        public UserItemResponse TechicalStaffId { get; set; }
    }

    public class ImageLivestockCircleResponse
    {
        public Guid Id { get; set; }
        public string Thumnail { get; set; }
        public string ImageLink { get; set; }
    }
}
