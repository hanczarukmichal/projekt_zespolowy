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
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetCurrentUserId() => _userManager.GetUserId(User);

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var transactions = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.ApplicationUserId == userId)
                .OrderByDescending(t => t.Date);
            return View(await transactions.ToListAsync());
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            var userId = GetCurrentUserId();
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.ApplicationUserId == userId), "Id", "Name");
            return View();
        }

        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Amount,Date,Type,CategoryId")] Transaction transaction)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            transaction.ApplicationUserId = user.Id;

            // Dla przychodu kategoria nie jest wymagana -> ustawiamy na null
            if (transaction.Type == TransactionType.Income)
            {
                transaction.CategoryId = null;
                ModelState.Remove("CategoryId");
                ModelState.Remove("Category");
            }

            ModelState.Remove("ApplicationUser");
            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.ApplicationUserId == user.Id), "Id", "Name", transaction.CategoryId);
            return View(transaction);
        }

        // ... (Reszta metod: Details, Edit, Delete analogicznie do poprzednich wersji)
    }
}