using projekt_zespolowy.Models;
using System;

namespace projekt_zespolowy.ViewModels
{
    public class BudgetStatusViewModel
    {
        public Budget Budget { get; set; }
        public decimal SpentAmount { get; set; }

        public decimal RemainingAmount => Budget.Amount - SpentAmount;
        public decimal Percentage => Budget.Amount > 0 ? (SpentAmount / Budget.Amount) * 100 : 0;
        public bool IsOverBudget => SpentAmount > Budget.Amount;
    }
}