using Xunit;
using Moq;
using glms.Services;
using glms.Interfaces;

namespace glms.Tests.Services
{
    public class CurrencyServiceTests
    {
        [Fact]
        public async Task ConvertCurrency_ShouldReturnCorrectValue()
        {
            // Arrange
            var mockRateProvider = new Mock<IExchangeRateProvider>();
            mockRateProvider
                .Setup(x => x.GetRate("USD", "ZAR"))
                .ReturnsAsync(18.5m);

            var service = new CurrencyService(mockRateProvider.Object);

            // Act
            var result = await service.ConvertCurrency(10, "USD", "ZAR");

            // Assert
            Assert.Equal(185m, result);
        }
        [Fact]
        public async Task ConvertCurrency_NegativeAmount_ShouldThrow()
        {
            var mockRateProvider = new Mock<IExchangeRateProvider>();
            var service = new CurrencyService(mockRateProvider.Object);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.ConvertCurrency(-1, "USD", "ZAR"));
        }
    }
}