using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class Medicine : EntityBase
    {
        public string MedicineName { get; set; }
        public Guid MedicineCategoryId { get; set; }
        public int Stock { get; set; }

        public virtual MedicineCategory MedicineCategory { get; set; }
    }
}
