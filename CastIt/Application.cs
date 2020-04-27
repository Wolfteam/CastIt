using CastIt.ViewModels;
using MvvmCross.IoC;
using MvvmCross.ViewModels;

namespace CastIt
{
    public class Application : MvxApplication
    {
        public override void Initialize()
        {
            base.Initialize();

            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            RegisterAppStart<MainViewModel>();
        }
    }
}
