using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utils.Grpc.DiscoveryClient;
using Utils.Grpc.Factory;
using Utils.Grpc.Interceptors;
using Utils.Grpc.Metrics;

namespace Utils.Grpc.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void UseChannelFactory(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DiscoveryConfiguration>(configuration.GetSection("DiscoveryService"));

            services.AddScoped<IDiscoveryServiceClient, DiscoveryServiceClient>();
            services.AddScoped<IMetricsProvider, PrometheusMetrics>();

            services.AddSingleton<IChannelFactory, ChannelFactory>();

            services.AddScoped<MetricsInterceptor>();

            services.AddSingleton<Operation>();

        }
    }
}
