using CastIt.Application.Server;
using CastIt.Cli.Common.Exceptions;
using CastIt.Cli.Interfaces.Api;
using CastIt.Domain.Dtos;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands
{
    [HelpOption("--help", ShortName = "h")]
    public abstract class BaseCommand
    {
        protected readonly IConsole AppConsole;
        protected readonly ICastItApiService CastItApi;
        protected const int SuccessCode = 0;
        protected const int ErrorCode = -1;

        protected BaseCommand(IConsole appConsole, ICastItApiService castItApi)
        {
            AppConsole = appConsole;
            CastItApi = castItApi;
        }

        protected virtual async Task<int> OnExecute(CommandLineApplication app)
        {
            try
            {
                return await Execute(app);
            }
            catch (Exception e)
            {
                return HandleCliException(e);
            }
        }

        protected virtual Task<int> Execute(CommandLineApplication app)
        {
            return Task.FromResult(SuccessCode);
        }

        protected void CheckIfWebServerIsRunning()
        {
            if (!WebServerUtils.IsServerAlive())
            {
                throw new ServerNotRunningException();
            }
        }

        protected void CheckServerResponse<T>(T response) where T : EmptyResponseDto
        {
            if (!response.Succeed)
            {
                throw new ServerApiException($"Something went wrong. Error = {response.Message}");
            }
        }

        protected int HandleCliException(Exception e)
        {
            if (e is BaseCliException baseCliException)
            {
                AppConsole.WriteLine(baseCliException.Message);
                return ErrorCode;
            }

            AppConsole.WriteLine($"Unknown error occurred. Error = {e.Message}");
            AppConsole.WriteLine(e.Message);
            AppConsole.WriteLine(e.StackTrace!);

            return ErrorCode;
        }

        protected void PrettyPrintAsJson(object something)
        {
            var json = JsonConvert.SerializeObject(something, Formatting.Indented);
            AppConsole.WriteLine(json);
        }
    }
}
