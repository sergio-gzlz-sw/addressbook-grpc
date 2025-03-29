﻿using System.ComponentModel.DataAnnotations;

namespace GrpcService.Models
{
    public class Person
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
