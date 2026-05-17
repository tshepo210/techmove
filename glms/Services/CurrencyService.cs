using glms.Interfaces;

namespace glms.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IExchangeRateProvider _rateProvider;

        public CurrencyService(IExchangeRateProvider rateProvider)
        {
            _rateProvider = rateProvider;
        }

        public async Task<decimal> ConvertCurrency(decimal amount, string from, string to)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative");

            var rate = await _rateProvider.GetRate(from, to);
            return amount * rate;
        }
    }
}