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
    [Table("Role")]
    public class Role : IdentityRole
    {
        public Role(string name) : base(name)
        {
        }
    }
}
