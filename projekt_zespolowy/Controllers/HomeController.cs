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

                var allTransactions = await _context.Transactions
                    .Where(t => t.ApplicationUserId == userId)
                    .ToListAsync();

                decimal balance = allTransactions
                    .Where(t => t.Date <= now && t.Type == TransactionType.Income).Sum(t => t.Amount) -
                    allTransactions
                    .Where(t => t.Date <= now && t.Type == TransactionType.Expense).Sum(t => t.Amount);

                var recentTransactions = allTransactions
                    .OrderByDescending(t => t.Date)
                    .Take(5)
                    .ToList();

                var budgets = await _context.Budgets
                    .Include(b => b.Category)
                    .Where(b => b.ApplicationUserId == userId && b.Month.Month == now.Month && b.Month.Year == now.Year)
                    .OrderByDescending(b => b.Priority)
                    .ToListAsync();

                var today = DateTime.Today;
                var monthAgo = today.AddDays(-30);

                var chartData = new List<object>();

                for (var date = monthAgo; date <= today; date = date.AddDays(1))
                {
                    var dayTrans = allTransactions.Where(t => t.Date.Date == date).ToList();

                    chartData.Add(new
                    {
                        Date = date.ToString("dd.MM"),
                        Income = dayTrans.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                        Expense = dayTrans.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                    });
                }

                ViewData["ChartData"] = System.Text.Json.JsonSerializer.Serialize(chartData);
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