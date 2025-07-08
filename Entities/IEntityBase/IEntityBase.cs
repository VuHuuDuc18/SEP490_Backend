using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityBase
{
    public interface IEntityBase
    {
        [Key] Guid Id { get; set; }
        [Required] bool IsActive { get; set; }

        [Required] DateTime CreatedDate { get; set;}
        DateTime? UpdatedDate { get; set;}
        [Required] Guid CreatedBy { get; set;}
        Guid? UpdatedBy { get; set;}
    }
}
