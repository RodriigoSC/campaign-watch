using Campaign.Watch.Infra.IoC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Campaign.Watch.Worker.Channels
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = ValidateIfNull(hostingContext.HostingEnvironment.EnvironmentName, "ASPNETCORE_ENVIRONMENT");

                    config.SetBasePath(AppContext.BaseDirectory)
                          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                          .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
                          .AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;

                    services.AddSingleton(configuration);

                    services.AddScoped<Worker>();

                    Bootstrap.StartIoC( services, configuration);

                    services.AddHostedService<Worker>();
                });

        private static string ValidateIfNull(string? value, string? name)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException($"{name} cannot be null");

            return value;
        }
    }
}
