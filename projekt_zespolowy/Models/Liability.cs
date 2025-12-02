using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace projekt_zespolowy.Models
{
    public enum LiabilityType
    {
        BankLoan,       // Pożyczka/Kredyt (Raty, Pasek postępu)
        FriendDebt      // Dług koleżeński (Prosty zwrot)
    }

    public class Liability
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa jest wymagana")]
        [Display(Name = "Nazwa / Cel")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Typ Zobowiązania")]
        public LiabilityType Type { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Kwota całkowita")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Już spłacono")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "Data zaciągnięcia")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Display(Name = "Termin spłaty")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        // Pola dla Pożyczek Bankowych
        [Display(Name = "Wysokość raty (miesięcznie)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? MonthlyInstallment { get; set; }

        // Pola dla Przypomnień
        [Display(Name = "Włącz przypomnienia")]
        public bool ReminderEnabled { get; set; }

        [Display(Name = "Opis / Notatka")]
        public string? Description { get; set; }

        // Helper do obliczania procentu
        public int ProgressPercentage
        {
            get
            {
                if (TotalAmount == 0) return 100;
                return (int)((PaidAmount / TotalAmount) * 100);
            }
        }
    }
}