using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using projekt_zespolowy.Data;
using projekt_zespolowy.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace projekt_zespolowy.Services
{
    public class AutoSaveService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AutoSaveService> _logger;

        public AutoSaveService(IServiceProvider serviceProvider, ILogger<AutoSaveService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AutoSave Service uruchomiony.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAutoSavings();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas przetwarzania automatycznych oszczędności.");
                }

                // Sprawdzaj co godzinę (lub rzadziej)
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task ProcessAutoSavings()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Pobierz cele, które mają włączony automat I ich data wykonania minęła lub jest dzisiaj
                var goalsToProcess = await context.SavingsGoals
                    .Where(g => g.IsAutoSaveEnabled && g.NextAutoSaveDate != null && g.NextAutoSaveDate <= DateTime.Now)
                    .ToListAsync();

                foreach (var goal in goalsToProcess)
                {
                    // 1. Zwiększ kwotę na celu
                    goal.CurrentAmount += goal.AutoSaveAmount;

                    // 2. Znajdź lub stwórz kategorię "Oszczędności" dla tego użytkownika
                    var category = await context.Categories
                        .FirstOrDefaultAsync(c => c.Name == "Oszczędności" && c.ApplicationUserId == goal.ApplicationUserId);

                    if (category == null)
                    {
                        category = new Category
                        {
                            Name = "Oszczędności",
                            ApplicationUserId = goal.ApplicationUserId
                        };
                        context.Categories.Add(category);
                        await context.SaveChangesAsync();
                    }

                    // 3. Utwórz transakcję (wydatek)
                    var transaction = new Transaction
                    {
                        ApplicationUserId = goal.ApplicationUserId,
                        Amount = goal.AutoSaveAmount,
                        Date = DateTime.Now,
                        Type = TransactionType.Expense,
                        Description = $"Automat: Wpłata na {goal.Name}",
                        CategoryId = category.Id
                    };
                    context.Transactions.Add(transaction);

                    // 4. Oblicz datę następnego przelewu (za miesiąc)
                    // Bierzemy aktualny NextAutoSaveDate i dodajemy miesiąc
                    var nextDate = goal.NextAutoSaveDate.Value.AddMonths(1);
                    goal.NextAutoSaveDate = nextDate;

                    context.Update(goal);
                    _logger.LogInformation($"Wykonano auto-zapis dla celu {goal.Name} kwota {goal.AutoSaveAmount}");
                }

                if (goalsToProcess.Any())
                {
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}