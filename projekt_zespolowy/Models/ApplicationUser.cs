using Microsoft.AspNetCore.Identity;

namespace projekt_zespolowy.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
    }
}