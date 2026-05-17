using System.Text.Json;
using glms.Interfaces;

namespace glms.Services
{
    public class ExchangeRateProvider : IExchangeRateProvider
    {
        private readonly HttpClient _httpClient;

        public ExchangeRateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetRate(string from, string to)
        {
            var response = await _httpClient.GetStringAsync(
                $"https://api.exchangerate-api.com/v4/latest/{from}");

            var data = JsonDocument.Parse(response);

            return data.RootElement
                       .GetProperty("rates")
                       .GetProperty(to)
                       .GetDecimal();
        }
    }
}