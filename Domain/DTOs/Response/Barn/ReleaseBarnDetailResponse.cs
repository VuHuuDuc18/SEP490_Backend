using Domain.Dto.Response.Breed;
using Domain.Dto.Response.LivestockCircle;
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

        public LivestockCircleResponse LivestockCircle { get; set; }
        public BreedResponse Breed { get; set; }
    }

 
    
}
