using Shouldly;
using Xunit;

namespace Pricer.PriceSource.Tests
{
    public class AlphaVantageDataSourceTest
    {
        [Fact] // (Skip = "Explicit - goes to external site")]
        public void Returns_a_stock()
        {
            var content = new AlphaVantageDataSource().GetDailyPrices("0200.HK", "QPYUOY7PYQ4L4OEP");
            content.Count.ShouldBeGreaterThan(0);
        }
    }
}
