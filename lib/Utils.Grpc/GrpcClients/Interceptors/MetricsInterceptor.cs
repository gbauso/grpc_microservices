using Utils.Grpc.Metrics;
using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Grpc.GrpcClients.Interceptors
{
    public class MetricsInterceptor : Interceptor
    {
        private readonly IMetricsProvider _metricsProvider;

        public MetricsInterceptor(IMetricsProvider metricsProvider)
        {
            _metricsProvider = metricsProvider;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
                                                                                      ClientInterceptorContext<TRequest, TResponse> context,
                                                                                      AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var requestStart = DateTime.UtcNow;
            var call = continuation(request, context);
            return new AsyncUnaryCall<TResponse>(
                call.ResponseAsync.ContinueWith(task =>
                {
                    Handled(MethodType.Unary, context, call.GetStatus().StatusCode, requestStart).Wait();
                    return task.Result;
                }),
                call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
        }

        
        private Task Handled<TRequest, TResponse>(MethodType type, ClientInterceptorContext<TRequest, TResponse> context, StatusCode status, DateTime requestStart)
            where TRequest : class
            where TResponse : class
            =>  _metricsProvider.CollectCallMetrics(new CallData {
                CallType = type.ToString().ToLower(),
                Duration = DateTime.UtcNow.Subtract(requestStart).TotalMilliseconds,
                Method = GetMethod(context),
                Status = status.ToString().ToLower()
            });

        private string GetMethod<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest : class
            where TResponse : class
        {
            var target = context.Options.Headers.FirstOrDefault(i => i.Key == "target").Value;
            var method = new StringBuilder();
            if (!string.IsNullOrEmpty(target)) method.Append($"{target}-");
            method.Append(context.Method.Name);

            return method.ToString();
        }
    }
} 
