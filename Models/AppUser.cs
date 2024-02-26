using Microsoft.AspNetCore.Identity;

namespace DemoWebAPI.Models
{
    public class AppUser : IdentityUser
    {
        public string DebitCardNumber { get; set; }
    }
}
