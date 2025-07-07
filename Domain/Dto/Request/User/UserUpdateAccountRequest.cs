using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Domain.Dto.Request.User
{
    public class UserUpdateAccountRequest
    {
        [EmailAddress]
        public string? Email { get; set; } = string.Empty;
        public string? FullName { get; set; } = string.Empty;
        [Phone]
        public string? Address { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
    }
} 