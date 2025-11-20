using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace projekt_zespolowy.Models
{
    public class Budget
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kwota jest wymagana")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Budżet musi być większy od 0")]
        [Display(Name = "Limit Budżetu")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Miesiąc i rok są wymagane")]
        [DataType(DataType.Date)]
        [Display(Name = "Miesiąc")]
        
        public DateTime Month { get; set; }

        [Required]
        [Display(Name = "Kategoria")]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}