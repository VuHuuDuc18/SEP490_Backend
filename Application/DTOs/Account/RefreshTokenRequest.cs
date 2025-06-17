using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.DTOs.Account
{
    public class RefreshTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
} 