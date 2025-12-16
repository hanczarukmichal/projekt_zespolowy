using System;
using System.Collections.Generic;

namespace projekt_zespolowy.ViewModels
{
    public class ReportViewModel
    {
        public DateTime CurrentMonth { get; set; }

        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance => TotalIncome - TotalExpense;

        public decimal PrevTotalIncome { get; set; }
        public decimal PrevTotalExpense { get; set; }

        public double IncomeChangePercent { get; set; }
        public double ExpenseChangePercent { get; set; }

        public List<string> ExpenseCategoryLabels { get; set; } = new List<string>();
        public List<decimal> ExpenseCategoryValues { get; set; } = new List<decimal>();

        public List<string> IncomeCategoryLabels { get; set; } = new List<string>();
        public List<decimal> IncomeCategoryValues { get; set; } = new List<decimal>();

        public List<string> DaysLabels { get; set; } = new List<string>();

        public List<decimal> CumulativeExpenseCurrent { get; set; } = new List<decimal>();
        public List<decimal> CumulativeExpensePrev { get; set; } = new List<decimal>();

        public List<decimal> CumulativeIncomeCurrent { get; set; } = new List<decimal>();
        public List<decimal> CumulativeIncomePrev { get; set; } = new List<decimal>();
    }

    public class CustomReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance => TotalIncome - TotalExpense;
        public IEnumerable<projekt_zespolowy.Models.Transaction> Transactions { get; set; }
    }
}