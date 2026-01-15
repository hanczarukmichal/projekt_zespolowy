using Microsoft.AspNetCore.Mvc;
using projekt_zespolowy.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace projekt_zespolowy.Controllers
{
    public class MarketController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MarketController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var model = new MarketDataViewModel();
            var client = _httpClientFactory.CreateClient();

            // Ustawienie User-Agent (wymagane przez niektóre API)
            client.DefaultRequestHeaders.Add("User-Agent", "SmartBudgetApp");

            // 1. POBIERANIE AKTUALNYCH KURSÓW WALUT (NBP)
            try
            {
                var response = await client.GetAsync("https://api.nbp.pl/api/exchangerates/tables/a/?format=json");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonDocument.Parse(content);
                    var rates = data.RootElement[0].GetProperty("rates");

                    foreach (var rate in rates.EnumerateArray())
                    {
                        var code = rate.GetProperty("code").GetString();
                        // Filtrujemy tylko te, które nas interesują
                        if (new[] { "USD", "EUR", "GBP", "CHF", "JPY" }.Contains(code))
                        {
                            model.Currencies.Add(new ExchangeRate
                            {
                                Code = code,
                                Currency = rate.GetProperty("currency").GetString(),
                                Mid = rate.GetProperty("mid").GetDecimal()
                            });
                        }
                    }
                }
            }
            catch { /* Logowanie błędu w razie potrzeby */ }

            

            await FetchStockData(model, client);

            return View(model);
        }

        private async Task FetchStockData(MarketDataViewModel model, HttpClient client)
        {
            string apiKey = "TWOJ_KLUCZ_STAD";
            string url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol=SPY&apikey={apiKey}";

            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonDocument.Parse(content);
                    var quote = data.RootElement.GetProperty("Global Quote");

                    model.Indices.Add(new StockIndex
                    {
                        Name = "S&P 500 (ETF)",
                        Price = quote.GetProperty("05. price").GetString(),
                        Change = quote.GetProperty("10. change percent").GetString()
                    });
                }
            }
            catch
            {
                // Dane zapasowe
                model.Indices.Add(new StockIndex { Name = "S&P 500", Price = "Błąd pobierania", Change = "0%" });
            }
        }
    }
}