namespace Pricer
{
    using Model;
    using PriceSource;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Collections.Generic;
    using System.Linq;

    public partial class MainWindow : Window
    {
        public MainWindow()
        {         
            InitializeComponent();

            var zeroPrice = new CurrentPrice();

            var tickers = new List<string>
            {
                "0200.HK", "0941.HK", "2318.HK"
            };

            var viewModel = new ViewModel
            {
                ApiKey = "",
                Stocks = new ObservableCollection<Stock>(
                    tickers.Select(ZeroPrice)
                    )
            };
            this.DataContext = viewModel;

            // an IoC container would usually be used here.
            var pricesSource = new AlphaVantageAPI(new AlphaVantageDataSource());
            var historicalDatabase = new InMemoryHistoricalDatabase();
            new ViewModelUpdater(pricesSource, historicalDatabase, viewModel);
        }

        private static Stock ZeroPrice(string ticker)
        {
            return new Stock
            {
                Ticker = ticker,
                Price = new CurrentPrice()
            };
        }
    }
}
