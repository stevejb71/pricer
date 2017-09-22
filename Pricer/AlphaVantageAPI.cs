using System;
using System.Collections.Generic;

namespace Pricer.PriceSource
{
    using Model;
    using RestSharp;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;

    public interface IAlphaVantageAPI
    {
        IObservable<CurrentPrice> GetLatestPrices(string ticker, string apiKey);

        List<HistoricalPrice> GetHistoricalPrices(string ticker, int daysOfHistory, string apiKey);
    }

    public class AlphaVantageAPI : IAlphaVantageAPI
    {
        private readonly AlphaVantageDataSource _dataSource;

        public AlphaVantageAPI(AlphaVantageDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public IObservable<CurrentPrice> GetLatestPrices(string ticker, string apiKey)
        {
            return Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(30), Scheduler.Default)
                .Select(_ => _dataSource.GetDailyPrices(ticker, apiKey))
                .Select(Parser.Parse)
                // catches (and drops) errors, which will happen with bad API Key
                .Catch(Observable.Empty<CurrentPrice>())
                .Distinct();
        }

        public List<HistoricalPrice> GetHistoricalPrices(string ticker, int daysOfHistory, string apiKey)
        {
            return Parser.ParseHistorical(_dataSource.GetDailyPrices(ticker, apiKey));
        }
    }

    public class AlphaVantageDataSource
    {
        public Dictionary<string, object> GetDailyPrices(string ticker, string apiKey)
        {
            var client = new RestClient("https://www.alphavantage.co");
            var request = new RestRequest("query", Method.GET);
            request.AddQueryParameter("function", "TIME_SERIES_DAILY");
            request.AddQueryParameter("symbol", ticker);
            request.AddQueryParameter("apikey", apiKey);

            return client.Execute<Dictionary<string, object>>(request).Data;
        }
    }

    internal static class Parser
    {
        internal static CurrentPrice Parse(Dictionary<string, object> raw)
        {
            var latestPrice = ParseHistorical(raw).First();
            return new CurrentPrice
            {
                LastTradePrice = latestPrice.ClosePrice,
                Volume = latestPrice.Volume
            };
        }

        internal static List<HistoricalPrice> ParseHistorical(Dictionary<string, object> raw)
        {
            var timeSeriesObj = (Dictionary<string, object>)raw["Time Series (Daily)"];
            return timeSeriesObj
                .ToDictionary(x => x.Key, x => (Dictionary<string, object>)x.Value)
                .Select(kv => new HistoricalPrice
                {
                    Date = Convert.ToDateTime(kv.Key),
                    ClosePrice = double.Parse((string)kv.Value["4. close"]),
                    Volume = int.Parse((string)kv.Value["5. volume"])
                })
                .ToList();
        }
    }
}
