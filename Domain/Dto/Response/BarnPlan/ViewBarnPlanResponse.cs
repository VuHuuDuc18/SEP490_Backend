using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.BarnPlan
{
    public class ViewBarnPlanResponse
    {
        public Guid Id { get; set; }
        public string Note { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<FoodPlan>? foodPlans { get; set; }
        public List<MedicinePlan>? medicinePlans { get; set; }
    }

    public class FoodPlan
    {
        public Guid FoodId { get; set; }
        public string FoodName { get; set; }
        public float Stock { get; set; }
        public string Note { get; set; }
    }
    public class MedicinePlan
    {
        public Guid MedicineId { get; set; }
        public string MedicineName { get; set; }
        public float Stock { get; set; }
        public string Note { get; set; }

    }
}
