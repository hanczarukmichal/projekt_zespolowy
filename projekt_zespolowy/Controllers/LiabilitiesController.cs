using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt_zespolowy.Data;
using projekt_zespolowy.Models;

namespace projekt_zespolowy.Controllers
{
    public class LiabilitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LiabilitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Liabilities
        // Ta akcja wyświetla listę pożyczek i długów
        public async Task<IActionResult> Index()
        {
            // Zabezpieczenie: jeśli tabela w bazie nie istnieje
            if (_context.Liabilities == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Liabilities' is null.");
            }

            var liabilities = await _context.Liabilities.ToListAsync();
            return View(liabilities);
        }

        // GET: Liabilities/Create
        // Ta akcja wyświetla formularz dodawania
        public IActionResult Create()
        {
            return View();
        }

        // POST: Liabilities/Create
        // Ta akcja odbiera dane z formularza i zapisuje w bazie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Type,TotalAmount,PaidAmount,StartDate,EndDate,MonthlyInstallment,ReminderEnabled,Description")] Liability liability)
        {
            if (ModelState.IsValid)
            {
                _context.Add(liability);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(liability);
        }

        // POST: Liabilities/PayInstallment/5
        // Szybka akcja do spłacania raty
        [HttpPost]
        public async Task<IActionResult> PayInstallment(int id)
        {
            var liability = await _context.Liabilities.FindAsync(id);
            if (liability == null) return NotFound();

            if (liability.MonthlyInstallment.HasValue)
            {
                liability.PaidAmount += liability.MonthlyInstallment.Value;

                // Nie pozwól spłacić więcej niż wynosi dług
                if (liability.PaidAmount > liability.TotalAmount)
                    liability.PaidAmount = liability.TotalAmount;

                _context.Update(liability);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Liabilities/Delete/5
        // Akcja usuwania (spłacenia całkowitego)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var liability = await _context.Liabilities.FindAsync(id);
            if (liability != null)
            {
                _context.Liabilities.Remove(liability);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}