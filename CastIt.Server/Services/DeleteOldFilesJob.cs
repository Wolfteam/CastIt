using System;
using System.Threading.Tasks;
using CastIt.Domain.Utils;
using CastIt.Shared.FilePaths;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CastIt.Server.Services;

[DisallowConcurrentExecution]
public class DeleteOldFilesJob : IJob
{
    private readonly ILogger _logger;
    private readonly IFileService _fileService;

    public DeleteOldFilesJob(ILoggerFactory loggerFactory, IFileService fileService)
    {
        _fileService = fileService;
        _logger = loggerFactory.CreateLogger<DeleteOldFilesJob>();
    }

    public Task Execute(IJobExecutionContext context)
    {
        const int days = 7;
        string path = AppFileUtils.GetServerLogsPath();

        _logger.LogInformation(
            "Starting job = {Job} at = {At} and deleting old files whose last access date <= {Days} from {Path}...",
            nameof(DeleteOldFilesJob), context.FireTimeUtc, days, path);

        _fileService.DeleteServerLogsAndPreviews(days * 2, days);

        _logger.LogInformation("Process completed at = {At}", DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }
}