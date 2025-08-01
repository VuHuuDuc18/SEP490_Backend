using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Dto.Request.Account
{
    public class UpdateAccountRequest
    {
        [Required]
        public string UserId { get; set; }
        [EmailAddress]
        public string? Email { get; set; } = string.Empty;
        public string? FullName { get; set; } = string.Empty;
        [Phone]
        public string? PhoneNumber { get; set; } = string.Empty;
    }
} 