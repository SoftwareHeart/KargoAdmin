using Microsoft.AspNetCore.Identity;

namespace KargoAdmin.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public ICollection<Blog> Blogs { get; set; }
    }
}
