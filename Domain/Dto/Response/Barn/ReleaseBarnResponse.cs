using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.Barn
{
    public class ReleaseBarnResponse
    {
        public Guid Id { get; set; }
        public string BarnName { get; set; }
        public string Address { get; set; }
        public string Image { get; set; }

        public int TotalUnit { get; set; }
        public int Age { get; set; }
        public int DeadUnit { get; set; }
        public int GoodUnitNumber { get; set; }
        public int BadUnitNumber { get; set; }
        public float AverageWeight { get; set; }
        public string BreedCategory{ get; set; }
        public string Breed{ get; set; }
        public DateTime? StartDate { get; set; }
    }
}
