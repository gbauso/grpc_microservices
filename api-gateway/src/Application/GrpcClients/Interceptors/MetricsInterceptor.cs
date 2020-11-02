using Application.Metrics;
using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Threading.Tasks;

namespace Application.GrpcClients.Interceptors
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
                CallType = type.ToString(),
                Duration = DateTime.UtcNow.Subtract(requestStart).TotalMilliseconds,
                Method = context.Method.Name,
                Status = status.ToString()
            });
    }
} 
