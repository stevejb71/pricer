using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Pricer.Model
{
    // Instance constructor would be nice here, c# 7 doesn't seem to be installed
    // by default.
    public class Stock : INotifyPropertyChanged
    {
        public string Ticker { get; set; }
        private CurrentPrice _price;
        public CurrentPrice Price
        {
            get
            {
                return _price;
            }
            set
            {
                _price = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Price"));
            }
        }
        public List<HistoricalPrice> HistoricalPrices { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class CurrentPrice
    {
        public double LastTradePrice { get; set; }
        public long Volume { get; set; }

        public override bool Equals(object obj)
        {
            var price = obj as CurrentPrice;
            return price != null &&
                   LastTradePrice == price.LastTradePrice &&
                   Volume == price.Volume;
        }

        public override int GetHashCode()
        {
            var hashCode = 542162481;
            hashCode = hashCode * -1521134295 + LastTradePrice.GetHashCode();
            hashCode = hashCode * -1521134295 + Volume.GetHashCode();
            return hashCode;
        }
    }

    public class HistoricalPrice
    {
        public DateTime Date { get; set; }
        public double LastTradePrice { get; set; }
        public long Volume { get; set; }
        public double ClosePrice { get; set; }
    }
}
