using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace projekt_zespolowy.Models
{
    public class SavingsGoal
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Podaj nazwę celu")]
        [Display(Name = "Nazwa celu")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Podaj kwotę celu")]
        [Display(Name = "Kwota docelowa")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TargetAmount { get; set; }

        [Display(Name = "Uzbierana kwota")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CurrentAmount { get; set; } = 0;

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        // --- NOWE POLA DLA AUTOMATYCZNEGO OSZCZĘDZANIA ---

        [Display(Name = "Automatyczne oszczędzanie")]
        public bool IsAutoSaveEnabled { get; set; } = false;

        [Display(Name = "Kwota miesięczna")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal AutoSaveAmount { get; set; } = 0;

        [Display(Name = "Dzień miesiąca (1-28)")]
        [Range(1, 28, ErrorMessage = "Wybierz dzień od 1 do 28")]
        public int AutoSaveDay { get; set; } = 1;

        // Pole techniczne - kiedy następny przelew?
        public DateTime? NextAutoSaveDate { get; set; }
    }
}