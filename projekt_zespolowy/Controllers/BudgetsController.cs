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
   // [Authorize] // Wymaga zalogowania
    public class BudgetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BudgetsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Budgets (Lista budżetów użytkownika)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var budgets = _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.ApplicationUserId == user.Id)
                .OrderByDescending(b => b.Month);

            return View(await budgets.ToListAsync());
        }

        // GET: Budgets/Create
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);
            // Pobierz tylko kategorie zalogowanego użytkownika
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.ApplicationUserId == userId), "Id", "Name");
            return View();
        }

        // POST: Budgets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Amount,Month,CategoryId")] Budget budget)
        {
            var user = await _userManager.GetUserAsync(User);

            // Walidacja: Sprawdź czy budżet dla tej kategorii w tym miesiącu już istnieje
            bool exists = await _context.Budgets.AnyAsync(b =>
                b.ApplicationUserId == user.Id &&
                b.CategoryId == budget.CategoryId &&
                b.Month.Month == budget.Month.Month &&
                b.Month.Year == budget.Month.Year);

            if (exists)
            {
                ModelState.AddModelError("", "Budżet dla tej kategorii w wybranym miesiącu już istnieje.");
            }

            if (ModelState.IsValid)
            {
                budget.ApplicationUserId = user.Id;
                // Ustawiamy zawsze na 1. dzień miesiąca dla spójności
                budget.Month = new DateTime(budget.Month.Year, budget.Month.Month, 1);

                _context.Add(budget);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.ApplicationUserId == user.Id), "Id", "Name", budget.CategoryId);
            return View(budget);
        }

        // GET: Budgets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.ApplicationUserId == user.Id);

            if (budget == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.ApplicationUserId == user.Id), "Id", "Name", budget.CategoryId);
            return View(budget);
        }

        // POST: Budgets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Amount,Month,CategoryId")] Budget budget)
        {
            if (id != budget.Id) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                try
                {
                    // Upewnij się, że edytowany obiekt należy do użytkownika
                    var existingBudget = await _context.Budgets.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id && b.ApplicationUserId == user.Id);
                    if (existingBudget == null) return NotFound();

                    budget.ApplicationUserId = user.Id;
                    budget.Month = new DateTime(budget.Month.Year, budget.Month.Month, 1);

                    _context.Update(budget);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BudgetExists(budget.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.ApplicationUserId == user.Id), "Id", "Name", budget.CategoryId);
            return View(budget);
        }

        // GET: Budgets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var budget = await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.ApplicationUserId == user.Id);

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

        private bool BudgetExists(int id)
        {
            return _context.Budgets.Any(e => e.Id == id);
        }
    }
}