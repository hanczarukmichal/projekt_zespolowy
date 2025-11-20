using System;
using System.Collections.Generic;
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
    [Authorize] // Blokuje dostęp dla niezalogowanych
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Metoda pomocnicza do pobierania ID aktualnego użytkownika
        private string GetCurrentUserId()
        {
            return _userManager.GetUserId(User);
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            // Wyświetl transakcje tylko zalogowanego użytkownika
            var transactions = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.ApplicationUserId == userId);

            return View(await transactions.ToListAsync());
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            // Sprawdź czy transakcja należy do użytkownika
            if (transaction.ApplicationUserId != GetCurrentUserId())
            {
                return Forbid();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            var userId = GetCurrentUserId();
            // Pokaż tylko kategorie należące do użytkownika
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.ApplicationUserId == userId), "Id", "Name");
            return View();
        }

        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Amount,Date,CategoryId")] Transaction transaction) // Usunięto "Type" z Bind
        {
            var userId = GetCurrentUserId();
            transaction.ApplicationUserId = userId;

            // Ustawiamy domyślny typ transakcji na Wydatek (Expense), skoro użytkownik nie wybiera go w formularzu
            transaction.Type = TransactionType.Expense;

            // Usuwamy walidację dla powiązanych obiektów i typu
            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("Category");
            ModelState.Remove("Type"); // Usuwamy walidację typu, bo ustawiamy go ręcznie

            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.ApplicationUserId == userId), "Id", "Name", transaction.CategoryId);
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            // Sprawdź uprawnienia
            if (transaction.ApplicationUserId != GetCurrentUserId())
            {
                return Forbid();
            }

            var userId = GetCurrentUserId();
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.ApplicationUserId == userId), "Id", "Name", transaction.CategoryId);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Amount,Date,Type,CategoryId")] Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            // Pobierz oryginał z bazy, żeby sprawdzić właściciela (bez śledzenia zmian na razie)
            var existingTransaction = await _context.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

            if (existingTransaction == null)
            {
                return NotFound();
            }

            if (existingTransaction.ApplicationUserId != GetCurrentUserId())
            {
                return Forbid();
            }

            // Upewnij się, że ID użytkownika się nie zmienia
            transaction.ApplicationUserId = GetCurrentUserId();

            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var userId = GetCurrentUserId();
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.ApplicationUserId == userId), "Id", "Name", transaction.CategoryId);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            if (transaction.ApplicationUserId != GetCurrentUserId())
            {
                return Forbid();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction != null)
            {
                if (transaction.ApplicationUserId != GetCurrentUserId())
                {
                    return Forbid();
                }

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            // Sprawdza istnienie transakcji należącej do tego użytkownika
            var userId = GetCurrentUserId();
            return _context.Transactions.Any(e => e.Id == id && e.ApplicationUserId == userId);
        }
    }
}