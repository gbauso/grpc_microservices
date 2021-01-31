using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utils.Grpc.Mediator.DiscoveryClient;
using Utils.Grpc.Mediator.Factory;
using Utils.Grpc.Mediator.GrpcClients;
using Utils.Grpc.Mediator.GrpcClients.Interceptors;
using Utils.Grpc.Mediator.Metrics;

namespace Utils.Grpc.Mediator.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void RegisterGrpcMediator(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DiscoveryConfiguration>(configuration.GetSection("DiscoveryService"));

            services.AddScoped<IDiscoveryServiceClient, DiscoveryServiceClient>();
            services.AddScoped<IMetricsProvider, PrometheusMetrics>();

            services.AddSingleton<ChannelFactory>();
            services.AddSingleton<ClientFactory>();

            services.AddScoped<MetricsInterceptor>();

            services.AddSingleton<Operation>();

            services.AddScoped<UnaryGrpcClientSingle>();

            services.AddTransient<IGrpcMediator, GrpcMediator>();
        }
    }
}
