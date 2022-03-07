namespace CastIt.Models
{
    public class NavigationResult<TResult>
    {
        public TResult Result { get; }

        public NavigationResult(TResult result)
        {
            Result = result;
        }

        public static NavigationResult<TResult> From(TResult result)
            => new NavigationResult<TResult>(result);
    }

    public class NavigationBoolResult : NavigationResult<bool>
    {
        public NavigationBoolResult(bool result) : base(result)
        {
        }

        public static NavigationBoolResult Succeed() => new NavigationBoolResult(true);

        public static NavigationBoolResult Fail() => new NavigationBoolResult(false);
    }
}
