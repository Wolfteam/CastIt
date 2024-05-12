using System;
using System.Threading.Tasks;
using CastIt.Server.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CastIt.Server.Services;

[DisallowConcurrentExecution]
public class SavePlayListAndFileChangesJob : IJob
{
    private readonly ILogger _logger;
    private readonly IServerCastService _serverCastService;

    public SavePlayListAndFileChangesJob(ILoggerFactory loggerFactory, IServerCastService serverCastService)
    {
        _serverCastService = serverCastService;
        _logger = loggerFactory.CreateLogger<SavePlayListAndFileChangesJob>();
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting job = {Job} at = {At}...", nameof(DeleteOldFilesJob), context.FireTimeUtc);
        await _serverCastService.SavePlayListAndFileChanges();
        _logger.LogInformation("Process completed at = {At}", DateTimeOffset.UtcNow);
    }
}