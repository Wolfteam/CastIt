using CastIt.Common;
using CastIt.Common.Utils;
using MvvmCross.Platforms.Wpf.Views;
using System;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CastIt
{
    public partial class App : MvxApplication<Setup, SetupApplication>
    {
        //https://stackoverflow.com/questions/14506406/wpf-single-instance-best-practices
        private static readonly string UniqueEventName = $"{AppConstants.AppName}_UniqueEvent";
        private static readonly string UniqueMutexName = $"{AppConstants.AppName}_UniqueMutex";

        private EventWaitHandle _eventWaitHandle;
        private Mutex _mutex;

        public App()
        {
            //https://docs.devexpress.com/WPF/400286/common-concepts/performance-improvement/reducing-the-application-launch-time
            // Defines where to store JIT profiles
            ProfileOptimization.SetProfileRoot(FileUtils.GetBaseAppFolder());
            // Enables Multicore JIT with the specified profile
            ProfileOptimization.StartProfile("Startup.Profile");
        }

        private void AppOnStartup(object sender, StartupEventArgs e)
        {
            _mutex = new Mutex(true, UniqueMutexName, out bool mutexCreated);
            _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

            // So, R# would not give a warning that this variable is not used.
            GC.KeepAlive(_mutex);

            if (mutexCreated)
            {
                // if this instance gets the signal to show the main window
                new Task(() =>
                {
                    while (_eventWaitHandle.WaitOne())
                    {
                        Current.Dispatcher.BeginInvoke((Action)(() => (Current.MainWindow as MainWindow)?.BringToForeground()));
                    }
                })
                .Start();
                return;
            }

            // Notify other instance so it could bring itself to foreground.
            _eventWaitHandle.Set();

            // Terminate this instance.
            Shutdown();
        }
    }
}
