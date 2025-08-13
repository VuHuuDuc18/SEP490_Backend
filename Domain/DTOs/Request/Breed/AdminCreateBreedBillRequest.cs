using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.Breed
{
    public class AdminCreateBreedBillRequest
    {
        public string LivestockCircleName {  get; set; }
        public DateTime DeliveryDate { get; set; }
        public Guid TechnicalStaffId { get; set; }
        public Guid BarnId {  get; set; }
        public Guid BreedId { get; set; }
        public string Note { get; set; }
       // public int AgeDays { get; set; }
        public int Stock {  get; set; }
    }
}
