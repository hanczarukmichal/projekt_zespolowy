using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt_zespolowy.Data;
using projekt_zespolowy.Models;
using projekt_zespolowy.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace projekt_zespolowy.Controllers
{
    [Authorize] // Tylko zalogowani użytkownicy
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, TransactionType? reportType)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Domyślne wartości: obecny miesiąc i Wydatki (Expense)
            var start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var end = endDate ?? DateTime.Now;
            var type = reportType ?? TransactionType.Expense;

            // Pobranie transakcji i grupowanie
            var query = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.ApplicationUserId == user.Id)
                .Where(t => t.Date >= start && t.Date <= end)
                .Where(t => t.Type == type);

            var groupedData = await query
                .GroupBy(t => t.Category != null ? t.Category.Name : "Brak kategorii")
                .Select(g => new CategorySum
                {
                    CategoryName = g.Key,
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToListAsync();

            var viewModel = new FinancialReportViewModel
            {
                StartDate = start,
                EndDate = end,
                ReportType = type,
                Data = groupedData
            };

            return View(viewModel);
        }
    }
}