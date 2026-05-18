using System;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;
using Xunit;
using glms.Services;
using glms.Interfaces;
using System.Net.Http;
using System.Text.Json;

namespace glms.Tests
{
    public class ExchangeRateProviderTests
    {
        [Fact]
        public async Task GetRate_ReturnsExpectedRate_ForUsdToZar()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();

            // Use a known rate to assert against
            var expectedRate = 18.50m;
            var jsonObj = new { @base = "USD", rates = new { ZAR = expectedRate }, date = "2026-05-18" };
            var json = JsonSerializer.Serialize(jsonObj);

            // Match the exact URL the provider calls
            mockHttp.When("https://api.exchangerate-api.com/v4/latest/USD")
                    .Respond("application/json", json);

            var httpClient = mockHttp.ToHttpClient();
            // The provider uses the absolute URL, BaseAddress is not required but harmless
            httpClient.BaseAddress = new Uri("https://api.exchangerate-api.com/");

            var provider = new ExchangeRateProvider(httpClient);

            // Act
            var rate = await provider.GetRate("USD", "ZAR");

            // Assert
            Assert.Equal(expectedRate, rate);
        }

        [Fact]
        public async Task ConvertUsdToZar_MultipliesAmountByRate()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();

            var rate = 18.50m;
            var jsonObj = new { @base = "USD", rates = new { ZAR = rate }, date = "2026-05-18" };
            var json = JsonSerializer.Serialize(jsonObj);
            mockHttp.When("https://api.exchangerate-api.com/v4/latest/USD")
                    .Respond("application/json", json);

            var httpClient = mockHttp.ToHttpClient();
            var provider = new ExchangeRateProvider(httpClient);

            var usdAmount = 123.45m;
            var expectedZar = usdAmount * rate;

            // Act
            var fetchedRate = await provider.GetRate("USD", "ZAR");
            var actualZar = usdAmount * fetchedRate;

            // Assert
            Assert.Equal(expectedZar, actualZar);
        }
    }
}
