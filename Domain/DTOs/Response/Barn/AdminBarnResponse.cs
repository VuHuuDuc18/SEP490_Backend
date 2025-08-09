
﻿using Domain.Dto.Response.LivestockCircle;
﻿using Domain.Dto.Response.Bill;
using Domain.Dto.Response.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.Barn
{
        // DTO để hiển thị danh sách chuồng trại cho admin
        public class AdminBarnResponse
        {
            public Guid Id { get; set; }
            public string BarnName { get; set; }
            public string Address { get; set; }
            public string Image { get; set; }
            public string? Description { get; set; }
            public WokerResponse Worker { get; set; }
            public bool IsActive { get; set; }
            public bool HasActiveLivestockCircle { get; set; } // Trạng thái có LivestockCircle đang hoạt động hay không
        }

        // DTO để hiển thị chi tiết chuồng trại cho admin
        public class AdminBarnDetailResponse
        {
            public Guid Id { get; set; }
            public string BarnName { get; set; }
            public string Address { get; set; }
            public string Image { get; set; }
            public string? Description { get; set; }
            public WokerResponse Worker { get; set; }
            public bool IsActive { get; set; }
            public ActiveLivestockCircleResponse? ActiveLivestockCircle { get; set; } // Thông tin LivestockCircle đang hoạt động (nếu có)
        }

       

        public class ActiveLivestockCircleResponse
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
            public UserItemResponse TechicalStaff { get; set; }
        }

    }

