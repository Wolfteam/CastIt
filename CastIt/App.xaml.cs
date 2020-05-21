using MvvmCross.Core;
using MvvmCross.Platforms.Wpf.Views;

namespace CastIt
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : MvxApplication
    {
        public App()
        {
            this.RegisterSetupType<Setup>();
        }
    }
}
