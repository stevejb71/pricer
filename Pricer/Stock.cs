using System;
using System.Collections.Generic;

namespace Pricer.Model
{
    // Instance constructor would be nice here, c# 7 doesn't seem to be installed
    // by default.
    public class Stock
    {
        public string Ticker { get; set; }
        public CurrentPrice Price { get; set; }
        public List<HistoricalPrice> HistoricalPrices { get; set; }
    }

    public class CurrentPrice
    {
        public double LastTradePrice { get; set; }
        public long Volume { get; set; }
    }

    public class HistoricalPrice
    {
        public DateTime Date { get; set; }
        public double LastTradePrice { get; set; }
        public long Volume { get; set; }
        public double ClosePrice { get; set; }
    }
}
