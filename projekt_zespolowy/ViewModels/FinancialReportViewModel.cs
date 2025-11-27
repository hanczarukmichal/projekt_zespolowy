using projekt_zespolowy.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace projekt_zespolowy.ViewModels
{
    public class FinancialReportViewModel
    {
        [DataType(DataType.Date)]
        [Display(Name = "Data początkowa")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Data końcowa")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Typ raportu")]
        public TransactionType ReportType { get; set; }

        public List<CategorySum> Data { get; set; } = new List<CategorySum>();

    }

  
    public class CategorySum
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}