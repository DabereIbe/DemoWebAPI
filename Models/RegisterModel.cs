using System.ComponentModel.DataAnnotations;

namespace DemoWebAPI.Models
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public string DebitCardNumber { get; set; }
    }
}
