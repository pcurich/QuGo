
using QuGo.Core.Configuration;

namespace QuGo.Core.Domain.Directory
{
    public class CurrencySettings : ISettings
    {
        public bool DisplayCurrencyLabel { get; set; }
        public int PrimaryApplicationCurrencyId { get; set; }
        public int PrimaryExchangeRateCurrencyId { get; set; }
        public string ActiveExchangeRateProviderSystemName { get; set; }
        public bool AutoUpdateEnabled { get; set; }
    }
}