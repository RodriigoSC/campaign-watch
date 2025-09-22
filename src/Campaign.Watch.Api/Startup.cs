using Campaign.Watch.Infra.IoC;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Campaign.Watch.Api
{
    /// <summary>
    /// Classe de inicializa��o da API, respons�vel por configurar os servi�os e o pipeline de requisi��es HTTP.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Inicializa uma nova inst�ncia da classe Startup.
        /// </summary>
        /// <param name="configuration">A configura��o da aplica��o injetada pelo host.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Obt�m a configura��o da aplica��o.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Este m�todo � chamado pelo runtime para adicionar servi�os ao cont�iner de inje��o de depend�ncia.
        /// </summary>
        /// <param name="services">A cole��o de servi�os para configurar.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Centraliza a inje��o de depend�ncia das outras camadas da aplica��o.
            Bootstrap.StartIoC(services, Configuration);

            services.AddControllers();

            // Configura o Swagger/OpenAPI para documenta��o da API.
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Campaign.Watch.Api", Version = "v1" });
            });
        }

        /// <summary>
        /// Este m�todo � chamado pelo runtime para configurar o pipeline de requisi��es HTTP.
        /// </summary>
        /// <param name="app">O construtor da aplica��o para configurar o pipeline de middleware.</param>
        /// <param name="env">O ambiente de hospedagem da aplica��o.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Em ambiente de desenvolvimento, habilita a p�gina de exce��es detalhadas e a UI do Swagger.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Campaign.Watch.Api v1"));
            }

            // Redireciona requisi��es HTTP para HTTPS.
            app.UseHttpsRedirection();

            // Habilita o roteamento.
            app.UseRouting();

            // Habilita a autoriza��o.
            app.UseAuthorization();

            // Mapeia os controllers para os endpoints da API.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}