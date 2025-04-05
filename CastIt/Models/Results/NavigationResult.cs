namespace CastIt.Models.Results
{
    public class NavigationBoolResult
    {
        public bool Value { get; }

        private NavigationBoolResult(bool value)
        {
            Value = value;
        }

        public static NavigationBoolResult Succeed() 
            => new NavigationBoolResult(true);

        public static NavigationBoolResult Fail() 
            => new NavigationBoolResult(false);
    }
}
