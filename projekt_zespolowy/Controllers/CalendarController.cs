using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt_zespolowy.Data;
using projekt_zespolowy.Models;

namespace projekt_zespolowy.Controllers
{
    [Authorize]
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CalendarController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents(DateTime start, DateTime end)
        {
            var user = await _userManager.GetUserAsync(User);
            var events = await _context.PaymentEvents
                .Where(e => e.ApplicationUserId == user.Id)
                .ToListAsync();

            var calendarEvents = new List<object>();

            foreach (var item in events)
            {
                string eventColor = item.IsPaid ? "#1cc88a" : "#f6c23e";

                if (!item.IsPaid && item.Date < DateTime.Now) eventColor = "#e74a3b";

                if (item.Frequency == PaymentFrequency.OneTime)
                {
                    if (item.Date >= start && item.Date <= end)
                        calendarEvents.Add(CreateEventObject(item, item.Date, eventColor));
                }
                else
                {
                    var currentDate = item.Date;
                    while (currentDate < start)
                        currentDate = item.Frequency == PaymentFrequency.Monthly ? currentDate.AddMonths(1) : currentDate.AddYears(1);

                    while (currentDate <= end)
                    {
                        calendarEvents.Add(CreateEventObject(item, currentDate, eventColor));
                        currentDate = item.Frequency == PaymentFrequency.Monthly ? currentDate.AddMonths(1) : currentDate.AddYears(1);
                    }
                }
            }

            return Json(calendarEvents);
        }

        private object CreateEventObject(PaymentEvent item, DateTime date, string color)
        {
            return new
            {
                id = item.Id,
                title = $"{item.Title} ({item.Amount:N0} zł)",
                start = date.ToString("yyyy-MM-dd"),
                allDay = true,
                color = color,
                extendedProps = new
                {
                    description = item.Description ?? "",
                    amount = item.Amount,
                    frequency = (int)item.Frequency,
                    isPaid = item.IsPaid,
                    originalDate = item.Date.ToString("yyyy-MM-dd")
                }
            };
        }

        // Metoda obsługująca ZAPIS i EDYCJĘ (Upsert)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([Bind("Id,Title,Amount,Date,Frequency,Description,IsPaid")] PaymentEvent paymentEvent)
        {
            var user = await _userManager.GetUserAsync(User);

            if (paymentEvent.Id == 0)
            {
                paymentEvent.ApplicationUserId = user.Id;
                ModelState.Remove("ApplicationUser");
                ModelState.Remove("ApplicationUserId");

                if (ModelState.IsValid)
                {
                    _context.Add(paymentEvent);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
            }
            else
            {
                var existing = await _context.PaymentEvents
                    .FirstOrDefaultAsync(e => e.Id == paymentEvent.Id && e.ApplicationUserId == user.Id);

                if (existing == null) return NotFound();

                existing.Title = paymentEvent.Title;
                existing.Amount = paymentEvent.Amount;
                existing.Date = paymentEvent.Date;
                existing.Frequency = paymentEvent.Frequency;
                existing.Description = paymentEvent.Description;
                existing.IsPaid = paymentEvent.IsPaid;

                await _context.SaveChangesAsync();
                return Ok();
            }

            return BadRequest("Błąd walidacji");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var ev = await _context.PaymentEvents.FirstOrDefaultAsync(e => e.Id == id && e.ApplicationUserId == user.Id);

            if (ev != null)
            {
                _context.PaymentEvents.Remove(ev);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var payment = await _context.PaymentEvents
                .FirstOrDefaultAsync(p => p.Id == id && p.ApplicationUserId == user.Id);

            if (payment == null) return NotFound();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == "Rachunki" && c.ApplicationUserId == user.Id);

            int? catId = category?.Id ?? (await _context.Categories.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id))?.Id;

            var transaction = new Transaction
            {
                ApplicationUserId = user.Id,
                Amount = payment.Amount,
                Date = DateTime.Now,
                Type = TransactionType.Expense,
                Description = $"Opłacono: {payment.Title}",
                CategoryId = catId
            };

            _context.Transactions.Add(transaction);

            if (payment.Frequency == PaymentFrequency.OneTime)
            {
                payment.IsPaid = true;
            }
            else
            {
                if (payment.Frequency == PaymentFrequency.Monthly)
                {
                    payment.Date = payment.Date.AddMonths(1);
                }
                else if (payment.Frequency == PaymentFrequency.Yearly)
                {
                    payment.Date = payment.Date.AddYears(1);
                }

                while (payment.Date < DateTime.Now)
                {
                    if (payment.Frequency == PaymentFrequency.Monthly) payment.Date = payment.Date.AddMonths(1);
                    else payment.Date = payment.Date.AddYears(1);
                }

                payment.IsPaid = false;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}