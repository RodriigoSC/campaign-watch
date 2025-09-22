using Campaign.Watch.Application.Interfaces.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Campaign.Watch.Worker.Channels
{
    /// <summary>
    /// Um servi�o de background que monitora campanhas em ciclos peri�dicos.
    /// Este worker � respons�vel por orquestrar a execu��o do fluxo de monitoramento,
    /// implementando l�gicas de retentativa, back-off em caso de falhas consecutivas e
    /// monitoramento de sua pr�pria sa�de operacional.
    /// </summary>
    public class Worker : BackgroundService
    {
        // Inje��es de depend�ncia e configura��es
        private readonly ICampaignMonitorFlow _campaignMonitorFlow;
        private readonly ILogger<Worker> _logger;

        // Configura��es do Worker
        private readonly TimeSpan _intervaloExecucao;
        private readonly bool _estaHabilitado;
        private readonly int _maxTentativas;
        private readonly TimeSpan _atrasoEntreTentativas;

        // Estado interno do Worker
        private DateTime _ultimaExecucaoComSucesso = DateTime.MinValue;
        private int _falhasConsecutivas = 0;

        /// <summary>
        /// Inicializa uma nova inst�ncia da classe Worker.
        /// </summary>
        /// <param name="fluxoMonitoramentoCampanha">O servi�o que cont�m a l�gica principal de monitoramento.</param>
        /// <param name="logger">A interface de log.</param>
        /// <param name="configuration">A configura��o da aplica��o para obter as defini��es do worker.</param>
        public Worker(ICampaignMonitorFlow fluxoMonitoramentoCampanha, ILogger<Worker> logger, IConfiguration configuration)
        {
            _campaignMonitorFlow = fluxoMonitoramentoCampanha;
            _logger = logger;

            // Carrega as configura��es do appsettings.json
            _intervaloExecucao = TimeSpan.FromMinutes(configuration.GetValue("WorkerSettings:ExecutionIntervalMinutes", 5));
            _estaHabilitado = configuration.GetValue("WorkerSettings:Enabled", true);
            _maxTentativas = configuration.GetValue("WorkerSettings:MaxRetryAttempts", 3);
            _atrasoEntreTentativas = TimeSpan.FromSeconds(configuration.GetValue("WorkerSettings:RetryDelaySeconds", 30));

            _logger.LogInformation(
                "Worker de Monitoramento de Campanhas configurado. " +
                "Habilitado: {EstaHabilitado}, Intervalo: {IntervaloExecucao} min, " +
                "M�ximo de Tentativas: {MaxTentativas}, Atraso entre Tentativas: {AtrasoEntreTentativas}s",
                _estaHabilitado, _intervaloExecucao.TotalMinutes, _maxTentativas, _atrasoEntreTentativas.TotalSeconds);
        }

        /// <summary>
        /// O m�todo principal do servi�o de background. Cont�m o loop que executa
        /// o monitoramento de campanhas enquanto o servi�o estiver ativo.
        /// </summary>
        /// <param name="stoppingToken">O token que sinaliza quando o servi�o deve ser interrompido.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_estaHabilitado)
            {
                _logger.LogWarning("O Worker est� desabilitado nas configura��es. A execu��o n�o ser� iniciada.");
                return;
            }

            _logger.LogInformation("Servi�o de monitoramento de campanhas iniciado.");

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Atraso inicial

            while (!stoppingToken.IsCancellationRequested)
            {
                var inicioExecucao = DateTime.UtcNow;
                var executionId = Guid.NewGuid().ToString("N")[..12];

                // Anexa o ExecutionId a todos os logs dentro deste bloco para facilitar o rastreamento.
                using (_logger.BeginScope(new Dictionary<string, object> { ["ExecutionId"] = executionId }))
                {
                    try
                    {
                        _logger.LogInformation("Iniciando novo ciclo de monitoramento...");

                        await ExecutarComTentativasAsync(stoppingToken);

                        // Reseta o estado de falha ap�s um ciclo bem-sucedido.
                        _ultimaExecucaoComSucesso = inicioExecucao;
                        _falhasConsecutivas = 0;

                        var duracao = DateTime.UtcNow - inicioExecucao;
                        _logger.LogInformation("Ciclo de monitoramento conclu�do com sucesso em {Duracao:F2}s.", duracao.TotalSeconds);

                        VerificarSaudeDoWorker();
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("O ciclo de monitoramento foi cancelado.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _falhasConsecutivas++;
                        _logger.LogError(ex, "Erro cr�tico n�o tratado no ciclo de monitoramento. Falhas consecutivas: {FalhasConsecutivas}", _falhasConsecutivas);

                        // Estrat�gia de back-off: Aumenta o tempo de espera se ocorrerem muitas falhas seguidas.
                        if (_falhasConsecutivas >= 5)
                        {
                            var atrasoEstendido = TimeSpan.FromMinutes(_intervaloExecucao.TotalMinutes * 2);
                            _logger.LogWarning("Muitas falhas consecutivas. Aguardando um tempo maior: {AtrasoEstendido} min", atrasoEstendido.TotalMinutes);
                            await AtrasarProximoCicloAsync(atrasoEstendido, stoppingToken);
                        }
                    }
                }

                await AtrasarProximoCicloAsync(_intervaloExecucao, stoppingToken);
            }

            _logger.LogInformation("Servi�o de monitoramento de campanhas finalizado.");
        }

        /// <summary>
        /// Tenta executar o fluxo principal de monitoramento com uma pol�tica de retentativas.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <exception cref="InvalidOperationException">Lan�ada se todas as tentativas de execu��o falharem.</exception>
        private async Task ExecutarComTentativasAsync(CancellationToken cancellationToken)
        {
            var tentativa = 1;
            Exception ultimaExcecao = null;

            while (tentativa <= _maxTentativas && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (tentativa > 1)
                        _logger.LogInformation("Iniciando tentativa {Tentativa}/{MaxTentativas}...", tentativa, _maxTentativas);
                    else
                        _logger.LogDebug("Executando o fluxo de monitoramento de campanhas.");

                    await _campaignMonitorFlow.MonitorarCampanhasAsync();
                    return; // Sucesso
                }
                catch (Exception ex)
                {
                    ultimaExcecao = ex;
                    _logger.LogWarning(ex, "Falha na tentativa {Tentativa}/{MaxTentativas}: {MensagemErro}", tentativa, _maxTentativas, ex.Message);

                    if (tentativa < _maxTentativas && !cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(_atrasoEntreTentativas, cancellationToken);
                    }
                    tentativa++;
                }
            }

            throw new InvalidOperationException($"Todas as {_maxTentativas} tentativas de execu��o falharam.", ultimaExcecao);
        }

        /// <summary>
        /// Realiza verifica��es peri�dicas da sa�de do worker e emite logs de status ou alerta.
        /// </summary>
        private void VerificarSaudeDoWorker()
        {
            var tempoDesdeUltimoSucesso = DateTime.UtcNow - _ultimaExecucaoComSucesso;

            if (_ultimaExecucaoComSucesso != DateTime.MinValue && tempoDesdeUltimoSucesso > TimeSpan.FromHours(1))
            {
                _logger.LogWarning(
                    "ALERTA DE SA�DE: Nenhuma execu��o foi conclu�da com sucesso h� {TempoDesdeUltimoSucesso:F1} horas. Falhas consecutivas: {FalhasConsecutivas}",
                    tempoDesdeUltimoSucesso.TotalHours, _falhasConsecutivas);
            }
            else
            {
                // A cada 30 minutos, se tudo estiver bem, registra um log de "sa�de".
                if (DateTime.UtcNow.Minute % 30 == 0)
                {
                    _logger.LogInformation("Status: Worker saud�vel. �ltimo sucesso em: {UltimaExecucaoComSucesso:yyyy-MM-dd HH:mm:ss} UTC", _ultimaExecucaoComSucesso);
                }
            }
        }

        /// <summary>
        /// Aguarda por um determinado per�odo de tempo de forma segura, respeitando o token de cancelamento.
        /// </summary>
        /// <param name="delay">O tempo de espera.</param>
        /// <param name="stoppingToken">O token de cancelamento.</param>
        private async Task AtrasarProximoCicloAsync(TimeSpan delay, CancellationToken stoppingToken)
        {
            try
            {
                if (delay > TimeSpan.Zero && !stoppingToken.IsCancellationRequested)
                {
                    _logger.LogDebug("Aguardando {Delay} para o pr�ximo ciclo.", delay);
                    await Task.Delay(delay, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Espera para o pr�ximo ciclo cancelada.");
            }
        }

        /// <summary>
        /// Chamado quando a aplica��o est� sendo finalizada para parar o servi�o.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recebido sinal para parar o servi�o...");
            await base.StopAsync(cancellationToken);
            _logger.LogInformation("Servi�o finalizado. �ltima execu��o com sucesso foi em: {UltimaExecucao}",
                 _ultimaExecucaoComSucesso == DateTime.MinValue ? "N/A" : _ultimaExecucaoComSucesso.ToString("yyyy-MM-dd HH:mm:ss UTC"));
        }
    }
}