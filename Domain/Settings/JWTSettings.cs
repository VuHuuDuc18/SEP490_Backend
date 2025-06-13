using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Settings
{
    public  class JWTSettings
    {
        public string SecurityKey { get; set; }
        public string LifeTime { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }

    }
}
