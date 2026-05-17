namespace glms.Interfaces
{
    public interface IExchangeRateProvider
    {
        Task<decimal> GetRate(string from, string to);
    }
}