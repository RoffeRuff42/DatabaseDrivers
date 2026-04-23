using TodoApi.DTOs;

namespace TodoApi.Services
{
    public  interface IQuoteService
    {
        Task<QuoteDto?> GetRandomQuote();
    }
}
