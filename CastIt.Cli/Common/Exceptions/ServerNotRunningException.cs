namespace CastIt.Cli.Common.Exceptions
{
    public class ServerNotRunningException : BaseCliException
    {
        public ServerNotRunningException(string message = "The web server is not running") : base(message)
        {
        }
    }
}
