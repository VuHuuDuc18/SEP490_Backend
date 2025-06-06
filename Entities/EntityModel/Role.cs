﻿using Entities.EntityBase;
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
    public class Role : EntityBase
    {
        [Required]
        public string RoleName { get; set; }
    }
}
