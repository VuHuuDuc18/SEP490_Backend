using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Dto.Request.Account
{
    public class RefreshTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
} 