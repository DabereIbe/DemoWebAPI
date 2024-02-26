using System.ComponentModel.DataAnnotations;

namespace DemoWebAPI.Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int Code { get; set; }
    }
}
