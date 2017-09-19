using System.Collections.Generic;

namespace Pricer
{
    using Model;

    public class ViewModel
    {
        public string ApiKey { get; set; }
        public List<Stock> Stocks { get; set; }
    }
}
