using System.ComponentModel.DataAnnotations;

namespace DemoWebAPI.Models.DTOs
{
    public class UsersDTO
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
