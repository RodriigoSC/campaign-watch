using Campaign.Watch.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Campaign.Watch.Worker.Channels
{
    public class Worker : BackgroundService
    {
        private readonly ICampaignMonitorFlow _campaignMonitorFlow;
        private readonly ILogger<Worker> _logger;

        public Worker(ICampaignMonitorFlow campaignMonitorFlow, ILogger<Worker> logger)
        {
            _campaignMonitorFlow = campaignMonitorFlow;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await _campaignMonitorFlow.MonitorCampaignsAsync();

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
