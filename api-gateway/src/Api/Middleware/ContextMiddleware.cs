using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Grpc.Experiments;

namespace Api.Middleware
{
    public class ContextMiddleware
    {
        private readonly RequestDelegate _next;

        public ContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.RequestServices.GetRequiredService<Operation>().OperationId = context.TraceIdentifier;

            await _next(context);
        }
    }
}
