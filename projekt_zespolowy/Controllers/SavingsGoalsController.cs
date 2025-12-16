using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt_zespolowy.Data;
using projekt_zespolowy.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace projekt_zespolowy.Controllers
{
    [Authorize]
    public class SavingsGoalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SavingsGoalsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: SavingsGoals
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var goals = await _context.SavingsGoals
                .Where(g => g.ApplicationUserId == user.Id)
                .ToListAsync();
            return View(goals);
        }

        // GET: SavingsGoals/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SavingsGoals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SavingsGoal savingsGoal)
        {
            // Ignorujemy walidację Usera, bo ustawiamy go ręcznie
            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("ApplicationUser");

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                savingsGoal.ApplicationUserId = user.Id;
                savingsGoal.CurrentAmount = 0;

                // LOGIKA DATY STARTOWEJ DLA AUTOMATU
                if (savingsGoal.IsAutoSaveEnabled)
                {
                    var today = DateTime.Now;
                    // Tworzymy datę z wybranego dnia w tym miesiącu

                    DateTime firstDate;
                    try
                    {
                        firstDate = new DateTime(today.Year, today.Month, savingsGoal.AutoSaveDay);
                    }
                    catch
                    {
                        // Fallback na 1. dzień, jeśli dzień miesiąca nie pasuje (np. 30 luty)
                        firstDate = new DateTime(today.Year, today.Month, 1);
                    }

                    // Jeśli ta data już była w tym miesiącu (np. dziś 15ty, a ustawiliśmy 10ty),
                    // to pierwszy przelew ustawiamy na przyszły miesiąc.
                    if (firstDate < today.Date)
                    {
                        firstDate = firstDate.AddMonths(1);
                    }

                    savingsGoal.NextAutoSaveDate = firstDate;
                }
                else
                {
                    savingsGoal.NextAutoSaveDate = null;
                }

                _context.Add(savingsGoal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(savingsGoal);
        }

        // --- METODA POMOCNICZA: Pobierz lub utwórz kategorię "Oszczędności" ---
        private async Task<Category> GetOrCreateSavingsCategory(string userId)
        {
            var categoryName = "Oszczędności";

            // 1. Szukamy czy kategoria już istnieje
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == categoryName && c.ApplicationUserId == userId);

            // 2. Jeśli nie istnieje - tworzymy ją
            if (category == null)
            {
                category = new Category
                {
                    Name = categoryName,
                    ApplicationUserId = userId
                };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync(); // Zapisujemy, żeby dostać ID
            }

            return category;
        }

        // AKCJA: WPŁATA (ODŁÓŻ PIENIĄDZE)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(int id, decimal amount)
        {
            if (amount <= 0) return RedirectToAction(nameof(Index));

            var user = await _userManager.GetUserAsync(User);
            var goal = await _context.SavingsGoals
                .FirstOrDefaultAsync(g => g.Id == id && g.ApplicationUserId == user.Id);

            if (goal == null) return NotFound();

            // 1. Zwiększ stan celu
            goal.CurrentAmount += amount;

            // 2. Pobierz lub stwórz kategorię "Oszczędności"
            var savingsCategory = await GetOrCreateSavingsCategory(user.Id);

            // 3. Utwórz transakcję (Wydatek z portfela)
            var transaction = new Transaction
            {
                ApplicationUserId = user.Id,
                Amount = amount,
                Date = DateTime.Now,
                Type = TransactionType.Expense,
                Description = $"Wpłata na cel: {goal.Name}",
                CategoryId = savingsCategory.Id // Tutaj przypisujemy ID kategorii "Oszczędności"
            };

            _context.Add(transaction);
            _context.Update(goal);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // AKCJA: WYPŁATA (WYCIĄGNIJ PIENIĄDZE)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int id, decimal amount)
        {
            if (amount <= 0) return RedirectToAction(nameof(Index));

            var user = await _userManager.GetUserAsync(User);
            var goal = await _context.SavingsGoals
                .FirstOrDefaultAsync(g => g.Id == id && g.ApplicationUserId == user.Id);

            if (goal == null) return NotFound();

            if (goal.CurrentAmount < amount)
            {
                TempData["Error"] = "Nie masz tyle środków na tym celu.";
                return RedirectToAction(nameof(Index));
            }

            // 1. Zmniejsz stan celu
            goal.CurrentAmount -= amount;

            // 2. Pobierz lub stwórz kategorię "Oszczędności"
            var savingsCategory = await GetOrCreateSavingsCategory(user.Id);

            // 3. Utwórz transakcję (Przychód do portfela)
            var transaction = new Transaction
            {
                ApplicationUserId = user.Id,
                Amount = amount,
                Date = DateTime.Now,
                Type = TransactionType.Income,
                Description = $"Wypłata z celu: {goal.Name}",
                CategoryId = savingsCategory.Id // Tutaj przypisujemy ID kategorii "Oszczędności"
            };

            _context.Add(transaction);
            _context.Update(goal);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: SavingsGoals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var savingsGoal = await _context.SavingsGoals
                .FirstOrDefaultAsync(m => m.Id == id && m.ApplicationUserId == user.Id);

            if (savingsGoal == null) return NotFound();

            return View(savingsGoal);
        }

        // POST: SavingsGoals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var savingsGoal = await _context.SavingsGoals
                .FirstOrDefaultAsync(m => m.Id == id && m.ApplicationUserId == user.Id);

            if (savingsGoal != null)
            {
                // LOGIKA ZWROTU ŚRODKÓW
                // Jeśli na celu są jakieś pieniądze, zwracamy je do portfela (tworzymy przychód)
                if (savingsGoal.CurrentAmount > 0)
                {
                    var savingsCategory = await GetOrCreateSavingsCategory(user.Id);

                    var transaction = new Transaction
                    {
                        ApplicationUserId = user.Id,
                        Amount = savingsGoal.CurrentAmount,
                        Date = DateTime.Now,
                        Type = TransactionType.Income, // Przychód - pieniądze wracają do dostępnych
                        Description = $"Zwrot środków z usuniętego celu: {savingsGoal.Name}",
                        CategoryId = savingsCategory.Id
                    };

                    _context.Transactions.Add(transaction);
                }

                // Usuwamy cel
                _context.SavingsGoals.Remove(savingsGoal);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}