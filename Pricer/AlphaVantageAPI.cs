﻿using System;
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

        List<HistoricalPrice> GetHistoricalPrices(int daysOfHistory);
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
                .Select(_ => _dataSource.GetCurrentPrice(ticker, apiKey))
                .Distinct();
        }

        public List<HistoricalPrice> GetHistoricalPrices(int daysOfHistory)
        {
            throw new NotImplementedException();
        }
    }

    public class AlphaVantageDataSource
    {
        public CurrentPrice GetCurrentPrice(string ticker, string apiKey)
        {
            var client = new RestClient("https://www.alphavantage.co");
            var request = new RestRequest("query", Method.GET);
            request.AddQueryParameter("function", "TIME_SERIES_WEEKLY");
            request.AddQueryParameter("symbol", ticker);
            request.AddQueryParameter("apikey", apiKey);

            var response = client.Execute<Dictionary<string, object>>(request);

            return Parser.Parse(response.Data);
        }
    }

    internal static class Parser
    {
        internal static CurrentPrice Parse(Dictionary<string, object> raw)
        {
            var timeSeries = (Dictionary<string, object>)raw["Weekly Time Series"];
            var latest = (Dictionary<string, object>)timeSeries.OrderBy(kv => kv.Key).First().Value;
            return new CurrentPrice
            {
                Volume = int.Parse((string)latest["5. volume"]),
                LastTradePrice = double.Parse((string)latest["4. close"])
            };
        }
    }
}
