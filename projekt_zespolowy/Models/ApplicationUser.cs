using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace projekt_zespolowy.Models
{
    public class ApplicationUser : IdentityUser
    {
        // --- TWOJE ISTNIEJĄCE POLA (Nie ruszamy ich) ---
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<Category> Categories { get; set; }

        // --- NOWE POLA (Dodajemy je dla Ustawień) ---

        [Display(Name = "Data Urodzenia")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Ścieżka do awatara")]
        public string? AvatarPath { get; set; } // Tu zapiszemy nazwę pliku, np. "avatar123.jpg"
    }
}