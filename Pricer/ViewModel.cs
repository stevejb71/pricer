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
        public ObservableCollection<HistoricalPrice> SelectedHistoricalPrices { get; set; }

        private Stock _selectedStock;
        public Stock SelectedStock
        {
            get
            {
                return _selectedStock;
            }
            set
            {
                if (_selectedStock == value) return;
                _selectedStock = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedStock"));
            }
        }

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
                switch(e.PropertyName)
                {
                    case "ApiKey":
                        Update(viewModel.ApiKey, viewModel.Stocks);
                        foreach(var stock in viewModel.Stocks)
                        {
                            var prices = priceDataSource.GetHistoricalPrices(stock.Ticker, 10, viewModel.ApiKey);
                            historicalDatabase.Update(stock.Ticker, prices);
                        }
                        break;
                    case "SelectedStock":
                        var historicalPrices = historicalDatabase.GetPrices(viewModel.SelectedStock.Ticker);
                        viewModel.SelectedHistoricalPrices.Clear();
                        historicalPrices.ForEach(viewModel.SelectedHistoricalPrices.Add);
                        break;
                    default:
                        throw new Exception($"Unhandled: {e.PropertyName}");
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
