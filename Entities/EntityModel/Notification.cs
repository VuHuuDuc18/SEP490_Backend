using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class Notification : EntityBase
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // info, warning, error, success...
        public bool IsRead { get; set; } = false;
        public bool IsNew { get; set; } = true;
        public DateTime? ReadDate { get; set; }
        public string? Link { get; set; }
        public object? Data { get; set; }
        public string? Icon { get; set; }
        public int Priority { get; set; }

        public virtual User User { get; set; }
    }
}
