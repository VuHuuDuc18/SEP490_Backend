using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request
{
    public class ListingRequest
    {
        public List<SearchObjectForCondition>? SearchString { get; set; }
        public List<SearchObjectForCondition>? Filter { get; set; }
        public SearchObjectForCondition? Sort { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
    public class SearchObjectForCondition
    {
        public string Field { get; set; }
        public string Value { get; set; }
    }
}
