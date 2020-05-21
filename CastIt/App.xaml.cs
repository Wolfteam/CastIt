using CastIt.Common.Utils;
using MvvmCross.Core;
using MvvmCross.Platforms.Wpf.Views;
using System.Runtime;

namespace CastIt
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : MvxApplication
    {
        public App()
        {
            //https://docs.devexpress.com/WPF/400286/common-concepts/performance-improvement/reducing-the-application-launch-time
            // Defines where to store JIT profiles
            ProfileOptimization.SetProfileRoot(FileUtils.GetBaseAppFolder());
            // Enables Multicore JIT with the specified profile
            ProfileOptimization.StartProfile("Startup.Profile");
            this.RegisterSetupType<Setup>();
        }
    }
}
