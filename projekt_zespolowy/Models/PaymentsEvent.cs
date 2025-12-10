using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace projekt_zespolowy.Models
{
    public enum PaymentFrequency
    {
        [Display(Name = "Jednorazowo")]
        OneTime,
        [Display(Name = "Co miesiąc")]
        Monthly,
        [Display(Name = "Co rok")]
        Yearly
    }

    public class PaymentEvent
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nazwa płatności")]
        public string Title { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Kwota")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Data płatności")]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Częstotliwość")]
        public PaymentFrequency Frequency { get; set; }

        public string? Description { get; set; }

        [Display(Name = "Czy opłacone?")]
        public bool IsPaid { get; set; }

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}