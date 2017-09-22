namespace Pricer
{
    using System.Collections.Generic;
    using Model;

    public interface IHistoricalDatabase
    {
        void Update(string ticker, List<HistoricalPrice> prices);

        List<HistoricalPrice> GetPrices(string ticker);
    }

    // I have limited time, and have not used a DB from C# before, 
    // so making it easier with this.
    public class InMemoryHistoricalDatabase : IHistoricalDatabase
    {
        private readonly Dictionary<string, List<HistoricalPrice>> _pricesByTicker = new Dictionary<string, List<HistoricalPrice>>();

        public List<HistoricalPrice> GetPrices(string ticker)
        {
            return _pricesByTicker[ticker];
        }

        public void Update(string ticker, List<HistoricalPrice> prices)
        {
            _pricesByTicker.Add(ticker, prices);
        }
    }
}
