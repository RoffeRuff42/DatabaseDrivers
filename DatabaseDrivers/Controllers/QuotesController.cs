using Microsoft.AspNetCore.Mvc;
using TodoApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace TodoApi.Controllers
{
    /// <summary>
    /// Controller for retrieving quotes from an external service.
    /// </summary>
    [ApiController]
    [Route("api/v1/quotes")]
    [Authorize]
    public class QuotesController : ControllerBase
    {
        private readonly IQuoteService _quoteService;

        public QuotesController(IQuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        /// <summary>
        /// Retrieves a random quote from the configured external quote provider.
        /// </summary>
        /// <returns>Returns 200 OK with a <see cref="TodoApi.DTOs.QuoteDto"/>, or 503 if the external API is unavailable.</returns>
        /// <response code="200">Successfully retrieved a quote.</response>
        /// <response code="503">External quote service is unavailable.</response>
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
