using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt_zespolowy.Data;
using projekt_zespolowy.Models;
using projekt_zespolowy.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace projekt_zespolowy.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- GŁÓWNY DASHBOARD RAPORTOWY ---
        public async Task<IActionResult> Index(int? month, int? year)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var now = DateTime.Now;
            int reportYear = year ?? now.Year;
            int reportMonth = month ?? now.Month;

            var currentMonthStart = new DateTime(reportYear, reportMonth, 1);
            var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);

            var prevMonthStart = currentMonthStart.AddMonths(-1);
            var prevMonthEnd = currentMonthStart.AddDays(-1);

            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.ApplicationUserId == user.Id)
                .Where(t => t.Date >= prevMonthStart && t.Date <= currentMonthEnd)
                .ToListAsync();

            var currentTrans = transactions.Where(t => t.Date >= currentMonthStart && t.Date <= currentMonthEnd).ToList();
            var prevTrans = transactions.Where(t => t.Date >= prevMonthStart && t.Date <= prevMonthEnd).ToList();

            var currentIncome = currentTrans.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var currentExpense = currentTrans.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            var prevIncome = prevTrans.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var prevExpense = prevTrans.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            var vm = new ReportViewModel
            {
                CurrentMonth = currentMonthStart,
                TotalIncome = currentIncome,
                TotalExpense = currentExpense,
                PrevTotalIncome = prevIncome,
                PrevTotalExpense = prevExpense,
                IncomeChangePercent = CalculateChange(currentIncome, prevIncome),
                ExpenseChangePercent = CalculateChange(currentExpense, prevExpense)
            };

            PreparePieChart(vm, currentTrans, TransactionType.Expense, true);
            PreparePieChart(vm, currentTrans, TransactionType.Income, false);

            int daysInMonth = DateTime.DaysInMonth(reportYear, reportMonth);
            decimal cumExpCurr = 0, cumExpPrev = 0;
            decimal cumIncCurr = 0, cumIncPrev = 0;

            for (int day = 1; day <= daysInMonth; day++)
            {
                vm.DaysLabels.Add(day.ToString());

                if (day <= currentMonthEnd.Day)
                {
                    cumExpCurr += currentTrans.Where(t => t.Type == TransactionType.Expense && t.Date.Day == day).Sum(t => t.Amount);
                    vm.CumulativeExpenseCurrent.Add(cumExpCurr);

                    cumIncCurr += currentTrans.Where(t => t.Type == TransactionType.Income && t.Date.Day == day).Sum(t => t.Amount);
                    vm.CumulativeIncomeCurrent.Add(cumIncCurr);
                }

                if (day <= DateTime.DaysInMonth(prevMonthStart.Year, prevMonthStart.Month))
                {
                    cumExpPrev += prevTrans.Where(t => t.Type == TransactionType.Expense && t.Date.Day == day).Sum(t => t.Amount);
                    cumIncPrev += prevTrans.Where(t => t.Type == TransactionType.Income && t.Date.Day == day).Sum(t => t.Amount);

                    vm.CumulativeExpensePrev.Add(cumExpPrev);
                    vm.CumulativeIncomePrev.Add(cumIncPrev);
                }
                else
                {
                    vm.CumulativeExpensePrev.Add(cumExpPrev);
                    vm.CumulativeIncomePrev.Add(cumIncPrev);
                }
            }

            return View(vm);
        }

        // --- RAPORT NIESTANDARDOWY ---
        public async Task<IActionResult> CustomReport(DateTime? startDate, DateTime? endDate)
        {
            var vm = new CustomReportViewModel();

            if (!startDate.HasValue) startDate = DateTime.Today.AddDays(-30);
            if (!endDate.HasValue) endDate = DateTime.Today;

            vm.StartDate = startDate.Value;
            vm.EndDate = endDate.Value;

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.ApplicationUserId == user.Id)
                .Where(t => t.Date >= vm.StartDate && t.Date <= vm.EndDate)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            vm.Transactions = transactions;
            vm.TotalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            vm.TotalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            return View(vm);
        }

        // Pomocnicze metody
        private double CalculateChange(decimal current, decimal previous)
        {
            if (previous == 0) return current == 0 ? 0 : 100;
            return (double)((current - previous) / previous) * 100;
        }

        private void PreparePieChart(ReportViewModel vm, List<Transaction> trans, TransactionType type, bool isExpense)
        {
            var groups = trans
                .Where(t => t.Type == type)
                .GroupBy(t => t.Category?.Name ?? "Inne")
                .Select(g => new { Name = g.Key, Amount = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Amount)
                .ToList();

            if (isExpense)
            {
                vm.ExpenseCategoryLabels = groups.Select(x => x.Name).ToList();
                vm.ExpenseCategoryValues = groups.Select(x => x.Amount).ToList();
            }
            else
            {
                vm.IncomeCategoryLabels = groups.Select(x => x.Name).ToList();
                vm.IncomeCategoryValues = groups.Select(x => x.Amount).ToList();
            }
        }
    }
}