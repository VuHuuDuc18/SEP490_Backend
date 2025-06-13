using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response
{
    public class MedicineResponse
    {
        public Guid Id { get; set; }
        public string MedicineName { get; set; }
        public Guid MedicineCategoryId { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
    }
}
