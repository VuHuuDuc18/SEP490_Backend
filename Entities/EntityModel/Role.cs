using Entities.EntityBase;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class Role : IdentityRole<Guid>, IEntityBase
    {
        [Required] 
        public bool IsActive { get; set; }

        [Required] 
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [Required] public Guid CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        
    }
}
