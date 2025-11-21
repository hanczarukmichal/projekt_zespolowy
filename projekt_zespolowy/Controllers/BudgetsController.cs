using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using projekt_zespolowy.Data;
using projekt_zespolowy.Models;

namespace projekt_zespolowy.Controllers
{
    [Authorize]
    public class BudgetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BudgetsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Budgets
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var budgets = _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.ApplicationUserId == user.Id)
                .OrderByDescending(b => b.Month);

            return View(await budgets.ToListAsync());
        }

        // GET: Budgets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Budgets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // ZMIANA: Dodano '?' przy string, aby parametry były opcjonalne
        public async Task<IActionResult> Create(Budget budget, string? CategorySelection, string? CustomCategoryName)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            budget.ApplicationUserId = user.Id;
            string finalCategoryName = "";

            // 1. Logika wyboru kategorii
            if (CategorySelection == "Custom")
            {
                if (string.IsNullOrWhiteSpace(CustomCategoryName))
                {
                    ModelState.AddModelError("CustomCategoryName", "Podaj nazwę własnej kategorii.");
                    ViewBag.CategorySelection = CategorySelection;
                    return View(budget);
                }
                finalCategoryName = CustomCategoryName;
            }
            else
            {
                // Jeśli wybrano kategorię predefiniowaną (np. Jedzenie), ignorujemy brak CustomCategoryName
                ModelState.Remove("CustomCategoryName"); // <--- KLUCZOWA POPRAWKA

                if (string.IsNullOrEmpty(CategorySelection)) CategorySelection = "Jedzenie";
                finalCategoryName = CategorySelection;

                budget.Priority = BudgetPriority.Wysoki;
            }

            // 2. Szukamy lub tworzymy kategorię
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == finalCategoryName && c.ApplicationUserId == user.Id);

            if (existingCategory == null)
            {
                var newCategory = new Category { Name = finalCategoryName, ApplicationUserId = user.Id };
                _context.Categories.Add(newCategory);
                await _context.SaveChangesAsync();
                budget.CategoryId = newCategory.Id;
            }
            else
            {
                budget.CategoryId = existingCategory.Id;
            }

            // 3. Czyszczenie walidacji pól systemowych
            ModelState.Remove("Category");
            ModelState.Remove("CategoryId");
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("Priority");
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                bool exists = await _context.Budgets.AnyAsync(b =>
                    b.ApplicationUserId == user.Id &&
                    b.CategoryId == budget.CategoryId &&
                    b.Month.Month == budget.Month.Month &&
                    b.Month.Year == budget.Month.Year);

                if (exists)
                {
                    ModelState.AddModelError("", $"Budżet dla kategorii '{finalCategoryName}' już istnieje w tym miesiącu.");
                    ViewBag.CategorySelection = CategorySelection;
                    return View(budget);
                }

                budget.Month = new DateTime(budget.Month.Year, budget.Month.Month, 1);

                _context.Add(budget);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategorySelection = CategorySelection;
            return View(budget);
        }

        // GET: Budgets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.GetUserAsync(User);
            var budget = await _context.Budgets.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id && b.ApplicationUserId == user.Id);
            if (budget == null) return NotFound();
            return View(budget);
        }

        // POST: Budgets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Budget budget)
        {
            if (id != budget.Id) return NotFound();
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            budget.ApplicationUserId = user.Id;

            ModelState.Remove("Category");
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("ApplicationUserId");

            if (ModelState.IsValid)
            {
                try
                {
                    var existsAndOwned = await _context.Budgets.AnyAsync(b => b.Id == id && b.ApplicationUserId == user.Id);
                    if (!existsAndOwned) return Forbid();

                    budget.Month = new DateTime(budget.Month.Year, budget.Month.Month, 1);
                    _context.Update(budget);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Budgets.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(budget);
        }

        // GET: Budgets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.GetUserAsync(User);
            var budget = await _context.Budgets.Include(b => b.Category).FirstOrDefaultAsync(m => m.Id == id && m.ApplicationUserId == user.Id);
            if (budget == null) return NotFound();
            return View(budget);
        }

        // POST: Budgets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.ApplicationUserId == user.Id);
            if (budget != null)
            {
                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}