using Application.Metrics;
using Metrics.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading;

namespace Metrics.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void ConfigureMetrics(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MetricsConfiguration>(configuration.GetSection("Metrics"));
            services.AddSingleton((provider) => {
                var candidate = provider.GetRequiredService<IOptions<MetricsConfiguration>>()?.Value;
                if (candidate.IsValid()) return candidate;

                var config = Newtonsoft.Json.JsonConvert.DeserializeObject<MetricsConfiguration>(
                                    configuration.GetValue<string>("Metrics")
                                    );
                return config;
            });
        }

        public static void AddInfluxDb(this IServiceCollection services, string service)
        {
            services.AddSingleton<ServerMetricsCollector>();
            services.AddSingleton<IMetricsProvider>((sp) => new InfluxDb(sp.GetRequiredService<MetricsConfiguration>(), service));
        }

        public static void StartServerCollect(this IApplicationBuilder app, double seconds)
        {
            new Timer(async (state) =>
            {
                await app.ApplicationServices.GetRequiredService<ServerMetricsCollector>().CollectServerMetrics(state);
            }
            , null, TimeSpan.Zero, TimeSpan.FromSeconds(seconds));
        }
    }
}
