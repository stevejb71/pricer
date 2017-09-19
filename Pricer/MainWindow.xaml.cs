using System.Windows;
using System.Collections.Generic;

namespace Pricer
{
    using Model;
    using PriceSource;
    using System.Collections.ObjectModel;

    public partial class MainWindow : Window
    {
        public MainWindow()
        {         
            InitializeComponent();

            var zeroPrice = new CurrentPrice();

            var viewModel = new ViewModel
            {
                ApiKey = "",
                Stocks = new ObservableCollection<Stock>
                {
                    ZeroPrice("0200.HK"),
                    ZeroPrice("0941.HK"),
                    ZeroPrice("2318.HK")
                }
            };
            this.DataContext = viewModel;

            // seems the best place to put this without using an IoC container.
            new ViewModelUpdater(new AlphaVantageAPI(new AlphaVantageDataSource()), viewModel);
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
