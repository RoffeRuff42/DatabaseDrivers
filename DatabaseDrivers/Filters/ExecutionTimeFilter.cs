using Microsoft.AspNetCore.Mvc.Filters;

namespace TodoApi.Filters
{
    public class ExecutionTimeFilter : IAsyncActionFilter
    {
        private readonly ILogger<ExecutionTimeFilter> _logger;
        public ExecutionTimeFilter(ILogger<ExecutionTimeFilter> logger)
        {
            _logger = logger;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var resultContext = await next();
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            _logger.LogInformation("Action {ActionName} executed in {ElapsedMilliseconds} ms",
                context.ActionDescriptor.DisplayName, elapsedMilliseconds);
        }
    }
}
