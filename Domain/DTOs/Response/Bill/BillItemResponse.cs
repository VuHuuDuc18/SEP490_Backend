using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Dto.Response.Bill
{
    public class BillItemResponse
    {
        public Guid Id { get; set; }
        //public Guid BillId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public FoodBillResponse? Food { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MedicineBillResponse? Medicine { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BreedBillResponse? Breed { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
    }

    public class FoodBillResponse
    {
        public Guid Id { get; set; }
        public string FoodName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Stock {  get; set; }
        public string Thumbnail { get; set; }
    }

    public class MedicineBillResponse
    {
        public Guid Id { get; set; }
        public string MedicineName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Stock { get; set; }
        public string Thumbnail { get; set; }
    }
    public class BreedBillResponse
    {
        public Guid Id { get; set; }
        public string BreedName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Stock { get; set; }
        public string Thumbnail { get; set; }
    }
}
