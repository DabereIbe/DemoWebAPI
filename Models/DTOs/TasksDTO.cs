﻿using System.ComponentModel.DataAnnotations;

namespace DemoWebAPI.Models.DTOs
{
    public class TasksDTO
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public bool IsComplete { get; set; }
    }
}
