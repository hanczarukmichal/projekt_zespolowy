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
        public async Task<IActionResult> Index(string searchString, int? categoryId, DateTime? dateFrom, DateTime? dateTo)
        {
            var userId = GetCurrentUserId();

            var transactions = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.ApplicationUserId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                if (decimal.TryParse(searchString, out decimal amount))
                {
                    transactions = transactions.Where(t => t.Amount == amount || t.Description.Contains(searchString));
                }
                else
                {
                    transactions = transactions.Where(t => t.Description.Contains(searchString));
                }
            }

            if (categoryId.HasValue)
            {
                transactions = transactions.Where(t => t.CategoryId == categoryId);
            }

            if (dateFrom.HasValue)
            {
                transactions = transactions.Where(t => t.Date >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                transactions = transactions.Where(t => t.Date <= dateTo.Value);
            }

            transactions = transactions.OrderByDescending(t => t.Date);

            ViewData["CurrentFilter"] = searchString;
            ViewData["DateFrom"] = dateFrom?.ToString("yyyy-MM-dd");
            ViewData["DateTo"] = dateTo?.ToString("yyyy-MM-dd");
            ViewData["SelectedCategoryId"] = categoryId;

            ViewData["Categories"] = new SelectList(_context.Categories.Where(c => c.ApplicationUserId == userId), "Id", "Name", categoryId);

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

    }
}