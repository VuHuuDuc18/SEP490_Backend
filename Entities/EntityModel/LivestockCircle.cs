using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class LivestockCircle : EntityBase
    {
        public string LivestockCircleName { get; set; }
        [Required]
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalUnit { get; set; }
        public int DeadUnit { get; set; }
        public float AverageWeight { get; set; }
        public int GoodUnitNumber { get; set; }
        public int BadUnitNumber { get; set; }

        public Guid BreedId { get; set; }
        public Guid BarnId { get; set; }
        public Guid TechicalStaffId { get; set; }   


        public virtual Breed Breed { get; set; }
        public virtual Barn Barn { get; set; }
        public virtual User TechicalStaff { get; set; }
    }
}
