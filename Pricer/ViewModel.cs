using System.Collections.Generic;
using System.Linq;

namespace Pricer
{
    using Model;
    using PriceSource;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private string _apiKey = "";
        public string ApiKey
        {
            get { return _apiKey; }
            set
            {
                if (_apiKey == value) return;
                _apiKey = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ApiKey"));
            }
        }
        public ObservableCollection<Stock> Stocks { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ViewModelUpdater
    {
        private readonly IAlphaVantageAPI _priceDataSource;
        private List<IDisposable> _subscriptions = new List<IDisposable>();

        public ViewModelUpdater(
            IAlphaVantageAPI priceDataSource,
            IHistoricalDatabase historicalDatabase,
            ViewModel viewModel
            )
        {
            _priceDataSource = priceDataSource;
            viewModel.PropertyChanged += (sender, e) => 
            {
                if(e.PropertyName == "ApiKey")
                {
                    Update(viewModel.ApiKey, viewModel.Stocks);
                    foreach(var stock in viewModel.Stocks)
                    {
                        var prices = priceDataSource.GetHistoricalPrices(stock.Ticker, 10, viewModel.ApiKey);
                        historicalDatabase.Update(stock.Ticker, prices);
                    }
                }
            };
        }

        private void Update(string apiKey, IEnumerable<Stock> stocks)
        {
            _subscriptions.ForEach(s => s.Dispose());
            foreach(var stock in stocks)
            {
                var sub = Subscribe(apiKey, stock);
                _subscriptions.Add(sub);
            }

        }

        private IDisposable Subscribe(string apiKey, Stock stock)
        {
            return _priceDataSource
                .GetLatestPrices(stock.Ticker, apiKey)
                .Subscribe(cp => {
                    stock.Price = cp;
                });
        }
    }
}
