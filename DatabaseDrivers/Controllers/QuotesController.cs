using Microsoft.AspNetCore.Mvc;
using TodoApi.Services;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/v1/quotes")]
    public class QuotesController : ControllerBase
    {
        private readonly IQuoteService _quoteService;

        public QuotesController(IQuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetQuote()
        {
            var quote = await _quoteService.GetRandomQuote();

            if (quote == null)
                return StatusCode(503, "External API unavailable");

            return Ok(quote);
        }
    }
}
