using CastIt.Cli.Interfaces.Api;
using CastIt.Domain.Dtos;
using McMaster.Extensions.CommandLineUtils;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Player
{
    [Command(
        Name = "goto",
        Description = "Goes to the next / previous file or it goes to a particular position / second in the current played file. The behaviour of this command depends on the params passed to it")]
    public class GoToCommand : BaseCommand
    {
        [Option(CommandOptionType.NoValue, Description = "If provided, it will play the next file in the current playlist", LongName = "next", ShortName = "next")]
        public bool Next { get; set; }

        [Option(CommandOptionType.NoValue, Description = "If provided, it will play the previous file in the current playlist", LongName = "previous", ShortName = "previous")]
        public bool Previous { get; set; }

        [Option(CommandOptionType.NoValue, Description = "If provided, it will go to the specified seconds provided in the value param", LongName = "seconds", ShortName = "seconds")]
        public bool Seconds { get; set; }

        [Option(CommandOptionType.NoValue, Description = "If provided, it will go to the specified position provided in the value param", LongName = "position", ShortName = "position")]
        public bool Position { get; set; }

        [Option(CommandOptionType.NoValue, Description = "If provided, it will add the specified amount of seconds provided in the value param", LongName = "seek", ShortName = "seek")]
        public bool Seek { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "The position / seconds value to go to", LongName = "value", ShortName = "value")]
        public double Value { get; set; }

        public GoToCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            CheckIfWebServerIsRunning();
            EmptyResponseDto response;
            if (Next)
            {
                AppConsole.WriteLine("Going to the next file in the current playlist...");
                response = await CastItApi.Next();
            }
            else if (Previous)
            {
                AppConsole.WriteLine("Going to the previous file in the current playlist...");
                response = await CastItApi.Previous();
            }
            else if (Seconds)
            {
                if (Value < 0)
                {
                    AppConsole.WriteLine($"The value = {Value} for seconds is not valid");
                    return ErrorCode;
                }
                AppConsole.WriteLine($"Going to = {Value} second(s) in the current file...");
                response = await CastItApi.GoToSeconds(Value);
            }
            else if (Position)
            {
                if (Value < 0 || Value > 100)
                {
                    AppConsole.WriteLine($"The value = {Value} for a position is not valid");
                    return ErrorCode;
                }
                AppConsole.WriteLine($"Going to = {Value} % in the current file...");
                response = await CastItApi.GoToPosition(Value);
            }
            else if (Seek)
            {
                if (Value <= 0)
                {
                    AppConsole.WriteLine($"The value = {Value} for a seek is not valid");
                    return ErrorCode;
                }
                response = await CastItApi.Seek(Value);
            }
            else
            {
                AppConsole.WriteLine("You need to provide at least one option for this command");
                return ErrorCode;
            }

            CheckServerResponse(response);

            return SuccessCode;
        }
    }
}
