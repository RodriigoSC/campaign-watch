using Campaign.Watch.Infra.IoC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Campaign.Watch.Worker.Channels
{
    /// <summary>
    /// A classe principal da aplica��o Worker Service.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// O ponto de entrada principal para a aplica��o.
        /// </summary>
        /// <param name="args">Argumentos de linha de comando.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Cria e configura o construtor do host da aplica��o (IHostBuilder).
        /// </summary>
        /// <param name="args">Argumentos de linha de comando passados para o construtor do host.</param>
        /// <returns>Uma inst�ncia de IHostBuilder configurada.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                // Configura as fontes de configura��o da aplica��o.
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = ValidateIfNull(hostingContext.HostingEnvironment.EnvironmentName, "ASPNETCORE_ENVIRONMENT");

                    config.SetBasePath(AppContext.BaseDirectory)
                          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                          .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
                          .AddEnvironmentVariables();
                })
                // Configura os servi�os da aplica��o e a inje��o de depend�ncia.
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;

                    // Registra a configura��o como um Singleton para ser acess�vel em toda a aplica��o.
                    services.AddSingleton(configuration);

                    // Adiciona a classe Worker principal como Scoped.
                    services.AddScoped<Worker>();

                    // Chama o m�todo de bootstrap para configurar a inje��o de depend�ncia das outras camadas.
                    Bootstrap.StartIoC(services, configuration);

                    // Registra a classe Worker como um servi�o hospedado (Hosted Service) para ser executado em background.
                    services.AddHostedService<Worker>();
                });

        /// <summary>
        /// Valida se um valor de string � nulo ou vazio.
        /// </summary>
        /// <param name="value">O valor a ser validado.</param>
        /// <param name="name">O nome da vari�vel, usado na mensagem de exce��o.</param>
        /// <returns>O valor original se n�o for nulo ou vazio.</returns>
        /// <exception cref="ArgumentNullException">Lan�ada se o valor for nulo ou vazio.</exception>
        private static string ValidateIfNull(string? value, string? name)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException($"{name} cannot be null");

            return value;
        }
    }
}