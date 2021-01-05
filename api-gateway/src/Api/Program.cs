using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.HashiCorpVault;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    configBuilder.AddEnvironmentVariables();
                    var configuration = configBuilder.Build();

                    var options = new VaultOptions();
                    configuration.Bind("VaultOptions", options);

                    var seeder = new VaultSeeder();
                    configuration.Bind("VaultSeeder", seeder);

                    configuration = configBuilder.AddHashiCorpVault(configuration).Build();
                    context.Configuration = configuration;
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
