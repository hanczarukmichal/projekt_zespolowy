using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace projekt_zespolowy.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}