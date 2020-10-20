using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace Api.Filter
{
    public class ErrorHandlerFilter : IAsyncExceptionFilter
    {
        private readonly ILogger<ErrorHandlerFilter> _logger;

        public ErrorHandlerFilter(ILogger<ErrorHandlerFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "There was an error");

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.HttpContext.Response.WriteAsync("There was an error");

        }
    }
}
