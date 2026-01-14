using System.Collections.Generic;

namespace projekt_zespolowy.Models
{
    public class MarketDataViewModel
    {
        public List<ExchangeRate> Currencies { get; set; } = new List<ExchangeRate>();
        public List<StockIndex> Indices { get; set; } = new List<StockIndex>();
    }

    public class ExchangeRate
    {
        public string Code { get; set; }
        public string Currency { get; set; }
        public decimal Mid { get; set; }
    }

    public class StockIndex
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public string Change { get; set; }
    }
}