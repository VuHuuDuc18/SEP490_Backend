using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.User
{
    public class UserItemResponse
    {
        public Guid UserId { get; set; }
        public string Fullname { get; set; }    
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

    }
}
