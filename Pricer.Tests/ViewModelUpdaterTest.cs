namespace Pricer.Tests
{
    using Shouldly;
    using Xunit;
    using PriceSource;
    using NSubstitute;
    using System.Collections.ObjectModel;
    using Pricer.Model;
    using System.Collections.Generic;
    using System;

    public class ViewModelUpdaterTest
    {
        private readonly IAlphaVantageAPI _priceDataSource = Substitute.For<IAlphaVantageAPI>();
        private readonly IHistoricalDatabase _historicalDatabase = Substitute.For<IHistoricalDatabase>();
        private readonly ViewModel _viewModel = new ViewModel();
        private ViewModelUpdater _viewModelUpdater;

        public ViewModelUpdaterTest()
        {
            _viewModelUpdater = new ViewModelUpdater(_priceDataSource, _historicalDatabase, _viewModel);
        }

        [Fact]
        public void Subscribes_to_latest_prices_when_the_apikey_changes()
        {
            var o200HK = new Stock { Ticker = "0200.HK" };
            _viewModel.Stocks = new ObservableCollection<Stock>(new List<Stock> { o200HK });
            var o200HKPrices = Substitute.For<IObservable<CurrentPrice>>();
            _priceDataSource.GetLatestPrices("0200.HK", "apiKey").Returns(o200HKPrices);

            _viewModel.ApiKey = "apiKey";

            o200HKPrices.Received().Subscribe(Arg.Any<IObserver<CurrentPrice>>());
        }

        [Fact]
        public void Subscription_updates_the_viewmodel_with_the_price_from_the_price_source()
        {
            var o200HK = new Stock { Ticker = "0200.HK" };
            _viewModel.Stocks = new ObservableCollection<Stock>(new List<Stock> { o200HK });
            var o200HKPrices = Substitute.For<IObservable<CurrentPrice>>();
            _priceDataSource.GetLatestPrices("0200.HK", "apiKey").Returns(o200HKPrices);
            IObserver<CurrentPrice> callback = null;
            o200HKPrices.Subscribe(Arg.Do<IObserver<CurrentPrice>>(x => callback = x));

            _viewModel.ApiKey = "apiKey";

            var newPrice = new CurrentPrice();
            callback.OnNext(newPrice);
            o200HK.Price.ShouldBeSameAs(newPrice);
        }

        [Fact]
        public void Disposes_of_outdated_current_price_subscriptions_when_api_key_changes()
        {
            var o200HK = new Stock { Ticker = "0200.HK" };
            _viewModel.Stocks = new ObservableCollection<Stock>(new List<Stock> { o200HK });
            var o200HKPrices = Substitute.For<IObservable<CurrentPrice>>();
            _priceDataSource.GetLatestPrices("0200.HK", "apiKey1").Returns(o200HKPrices);
            var oldSubscription = Substitute.For<IDisposable>();
            o200HKPrices.Subscribe(Arg.Any<IObserver<CurrentPrice>>()).Returns(oldSubscription);

            _viewModel.ApiKey = "apiKey1";
            _viewModel.Stocks = new ObservableCollection<Stock>();
            _viewModel.ApiKey = "apiKey2";

            oldSubscription.Received().Dispose();
        }

        [Fact]
        public void Updates_the_historical_database_with_10d_of_data_when_the_apikey_changes()
        {
            var o200HK = new Stock { Ticker = "0200.HK" };
            _viewModel.Stocks = new ObservableCollection<Stock>(new List<Stock> { o200HK });
            var historicalPrices = new List<HistoricalPrice> { Substitute.For<HistoricalPrice>() };
            _priceDataSource.GetHistoricalPrices("0200.HK", 10, "apiKey").Returns(historicalPrices);

            _viewModel.ApiKey = "apiKey";

            _historicalDatabase.Received().Update("0200.HK", historicalPrices);
        }
    }

}
