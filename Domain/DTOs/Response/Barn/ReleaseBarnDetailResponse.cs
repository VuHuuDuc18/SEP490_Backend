using Domain.Dto.Response.Breed;
using Entities.EntityModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.Barn
{
    public class ReleaseBarnDetailResponse
    {
        public Guid Id { get; set; }
        public string BarnName { get; set; }
        public string Address { get; set; }
        public string Image { get; set; }

        public LiveStockCircleResponse LiveStockCircle { get; set; }
        public BreedResponse Breed { get; set; }
    }
    public class LiveStockCircleResponse
    {
        public string LivestockCircleName { get; set; }
        public DateTime? StartDate { get; set; }
        public int TotalUnit { get; set; }
        public int DeadUnit { get; set; }
        public float AverageWeight { get; set; }
        public int GoodUnitNumber { get; set; }
        public int BadUnitNumber { get; set; }
        public List<ImageLivestockCircleResponse>? Images {  get; set; }
    }
    public class ImageLivestockCircleResponse
    {
        public Guid Id {  get; set; }
        public string Thumnail { get; set; }
        public string ImageLink { get; set; }
    }
    
}
