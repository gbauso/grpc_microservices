using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utils.Grpc.DiscoveryClient;
using Utils.Grpc.Factory;
using Utils.Grpc.GrpcClients;
using Utils.Grpc.GrpcClients.Interceptors;
using Utils.Grpc.Metrics;

namespace Utils.Grpc.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void RegisterGrpcMediator(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DiscoveryConfiguration>(configuration.GetSection("DiscoveryService"));

            services.AddSingleton<IDiscoveryServiceClient, DiscoveryServiceClient>();
            services.AddSingleton<IMetricsProvider, PrometheusMetrics>();

            services.AddSingleton<ChannelFactory>();
            services.AddSingleton<ClientFactory>();

            services.AddSingleton<MetricsInterceptor>();

            services.AddSingleton<Operation>();

            services.AddScoped<IGrpcClient, UnaryGrpcClientSingle>();
        }
    }
}
