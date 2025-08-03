using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.BarnPlan
{
    public class UpdateBarnPlanRequest
    {
        public Guid Id { get; set; }
        public string Note { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }        
        public List<FoodPlan>? foodPlans { get; set; }
        public List<MedicinePlan>? medicinePlans { get; set; }
    }
}
