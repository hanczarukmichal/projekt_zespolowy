using System.ComponentModel.DataAnnotations;

namespace projekt_zespolowy.Models
{
    public class SettingsViewModel
    {
        [Display(Name = "Nazwa użytkownika")]
        public string? UserName { get; set; } // Zmienione na nullable (?), aby uniknąć błędów walidacji

        [Display(Name = "Numer telefonu")]
        [Phone]
        [RegularExpression(@"^\d+$", ErrorMessage = "Numer telefonu może składać się tylko z cyfr.")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Data urodzenia")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Twój Awatar")]
        public string? CurrentAvatarPath { get; set; }

        [Display(Name = "Zmień zdjęcie")]
        public IFormFile? AvatarFile { get; set; }
    }
}