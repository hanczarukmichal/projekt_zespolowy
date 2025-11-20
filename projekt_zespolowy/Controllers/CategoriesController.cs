using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using projekt_zespolowy.Data;
using projekt_zespolowy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace projekt_zespolowy.Controllers
{
    [Authorize] // Wymaga zalogowania dla wszystkich akcji w tym kontrolerze
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoriesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Metoda pomocnicza do pobierania ID zalogowanego użytkownika
        private string GetCurrentUserId()
        {
            return _userManager.GetUserId(User);
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            // Filtrujemy kategorie, aby pokazać tylko te należące do zalogowanego użytkownika
            var userCategories = _context.Categories
                .Where(c => c.ApplicationUserId == currentUserId);

            return View(await userCategories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            // Sprawdzamy, czy kategoria należy do użytkownika
            if (category.ApplicationUserId != GetCurrentUserId())
            {
                return Forbid(); // Brak dostępu
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Category category)
        {
            // Przypisujemy kategorię do aktualnie zalogowanego użytkownika
            category.ApplicationUserId = GetCurrentUserId();

            // Usuwamy błędy walidacji dla pól użytkownika (bo ustawiamy je ręcznie)
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("ApplicationUserId");

            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Zabezpieczenie: użytkownik może edytować tylko swoje kategorie
            if (category.ApplicationUserId != GetCurrentUserId())
            {
                return Forbid();
            }

            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            // Pobieramy oryginalną kategorię z bazy, aby sprawdzić uprawnienia
            var categoryToUpdate = await _context.Categories.FindAsync(id);

            if (categoryToUpdate == null)
            {
                return NotFound();
            }

            // Sprawdzamy, czy kategoria należy do użytkownika
            if (categoryToUpdate.ApplicationUserId != GetCurrentUserId())
            {
                return Forbid();
            }

            // Aktualizujemy tylko nazwę
            categoryToUpdate.Name = category.Name;

            // Usuwamy walidację dla użytkownika
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("ApplicationUserId");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoryToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            if (category.ApplicationUserId != GetCurrentUserId())
            {
                return Forbid();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category != null)
            {
                // Sprawdzenie uprawnień przed usunięciem
                if (category.ApplicationUserId != GetCurrentUserId())
                {
                    return Forbid();
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            var currentUserId = GetCurrentUserId();
            return _context.Categories.Any(e => e.Id == id && e.ApplicationUserId == currentUserId);
        }
    }
}