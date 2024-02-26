using System.ComponentModel.DataAnnotations;

namespace DemoWebAPI.Models
{
    public class Tasks
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public bool IsComplete { get; set; }

        public DateTime DateAdded { get; set; }

        public DateTime? DateModified { get; set; } 

        public DateTime? DateCompleted { get; set; }


    }
}
