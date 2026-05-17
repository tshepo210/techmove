namespace glms.Interfaces
{
    public interface ICurrencyService
    {
        Task<decimal> ConvertCurrency(decimal amount, string from, string to);
    }
}