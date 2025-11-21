using Microsoft.AspNetCore.Mvc;
using projekt_zespolowy.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using projekt_zespolowy.Data;
using Microsoft.EntityFrameworkCore;

namespace projekt_zespolowy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user.Id;
                var now = DateTime.Now;

                // 1. Pobierz WSZYSTKIE transakcje
                var allTransactions = await _context.Transactions
                    .Where(t => t.ApplicationUserId == userId)
                    .ToListAsync();

                // Saldo: Przychody - Wydatki (tylko te, które ju¿ nast¹pi³y <= now)
                decimal balance = allTransactions
                    .Where(t => t.Date <= now && t.Type == TransactionType.Income).Sum(t => t.Amount) -
                    allTransactions
                    .Where(t => t.Date <= now && t.Type == TransactionType.Expense).Sum(t => t.Amount);

                // 2. Ostatnie transakcje (w tym przysz³e/zaplanowane)
                var recentTransactions = allTransactions
                    .OrderByDescending(t => t.Date)
                    .Take(5)
                    .ToList();

                // 3. Bud¿ety posortowane po PRIORYTECIE (Wysoki -> Niski)
                var budgets = await _context.Budgets
                    .Include(b => b.Category)
                    .Where(b => b.ApplicationUserId == userId && b.Month.Month == now.Month && b.Month.Year == now.Year)
                    .OrderByDescending(b => b.Priority)
                    .ThenByDescending(b => b.Amount)
                    .ToListAsync();

                ViewData["MainBalance"] = balance;
                ViewData["RecentTransactions"] = recentTransactions;
                ViewData["Budgets"] = budgets;

                return View("Dashboard");
            }
            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}