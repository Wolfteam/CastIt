using CastIt.Common.Enums;
using CastIt.Common.Extensions;
using CastIt.Interfaces;
using CastIt.ViewModels;
using CastIt.Views;
using MaterialDesignThemes.Wpf;
using MvvmCross;
using System;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;

namespace CastIt.Common.Utils
{
    public static class MinimizeToTray
    {
        public static void Enable(MainWindow window)
        {
            // No need to track this instance; its event handlers will keep it alive
            new MinimizeToTrayInstance(window);
        }

        //TODO: CANT REMOVE THE BLUE BORDER 
        private class MinimizeToTrayInstance
        {
            private readonly IAppSettingsService _settingsService;
            private readonly MainWindow _window;
            private NotifyIcon _notifyIcon;
            private bool _balloonShown;

            public MainViewModel MainViewModel
                => (_window.Content as MainPage).ViewModel;

            public MinimizeToTrayInstance(MainWindow window)
            {
                _settingsService = Mvx.IoCProvider.Resolve<IAppSettingsService>();
                _window = window;
                _window.StateChanged += HandleStateChanged;
            }

            private void HandleStateChanged(object sender, EventArgs e)
            {
                if (!_settingsService.MinimizeToTray)
                    return;

                if (_notifyIcon == null)
                {
                    _notifyIcon = new NotifyIcon()
                    {
                        Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location)
                    };

                    _notifyIcon.DoubleClick += NotifyIconDoubleClicked;
                }
                SetContextMenuItems();

                // Update copy of Window Title in case it has changed
                _notifyIcon.Text = _window.Title;

                // Show/hide Window and NotifyIcon
                var minimized = _window.WindowState == WindowState.Minimized;
                _window.ShowInTaskbar = !minimized;
                _notifyIcon.Visible = minimized;
                if (minimized && !_balloonShown)
                {
                    // If this is the first time minimizing to the tray, show the user what happened
                    _notifyIcon.ShowBalloonTip(1000, null, _window.Title, ToolTipIcon.None);
                    _balloonShown = true;
                }
            }
            private void SetContextMenuItems()
            {
                var brush = _settingsService.AppTheme == AppThemeType.Dark
                    ? System.Windows.Media.Brushes.White
                    : System.Windows.Media.Brushes.Black;
                var pen = _settingsService.AppTheme == AppThemeType.Dark
                    ? new System.Windows.Media.Pen(System.Windows.Media.Brushes.Black, 0.5)
                    : new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 0.5);

                var accentColor = (System.Windows.Application.Current.Resources["PrimaryHueDarkBrush"] as System.Windows.Media.SolidColorBrush)
                    .Color.ToDrawingColor();
                var fontColor = (System.Windows.Application.Current.Resources["FontColorBrush"] as System.Windows.Media.SolidColorBrush)
                    .Color.ToDrawingColor();
                var bgColor = (System.Windows.Application.Current.Resources["WindowBackground"] as System.Windows.Media.SolidColorBrush)
                    .Color.ToDrawingColor();

                var renderer = new CustomRenderer(fontColor, bgColor, accentColor)
                {
                    RoundedEdges = true
                };

                _notifyIcon.ContextMenuStrip = new ContextMenuStrip
                {
                    Renderer = renderer,
                    //RenderMode = ToolStripRenderMode.System,
                    BackColor = bgColor,
                    ForeColor = fontColor,
                    //Padding = new Padding(0),
                    //Margin = new Padding(0)
                };

                _notifyIcon.ContextMenuStrip.Items.Add(
                    MainViewModel.GetText("ShowMainWindow"),
                    WindowsUtils.GetImage(PackIconKind.WindowRestore, brush, pen),
                    NotifyIconDoubleClicked);
                _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                _notifyIcon.ContextMenuStrip.Items.Add(
                    $"{MainViewModel.GetText("Play")} / {MainViewModel.GetText("Pause")}",
                    WindowsUtils.GetImage(PackIconKind.Play, brush, pen),
                    TogglePlayBack);
                _notifyIcon.ContextMenuStrip.Items.Add(
                    MainViewModel.GetText("Stop"),
                    WindowsUtils.GetImage(PackIconKind.Stop, brush, pen),
                    StopPlayBack);
                _notifyIcon.ContextMenuStrip.Items.Add(
                    MainViewModel.GetText("Next"),
                    WindowsUtils.GetImage(PackIconKind.SkipNext, brush, pen),
                    PlayNext);
                _notifyIcon.ContextMenuStrip.Items.Add(
                    MainViewModel.GetText("Previous"),
                    WindowsUtils.GetImage(PackIconKind.SkipPrevious, brush, pen),
                    PlayPrevious);
                _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                _notifyIcon.ContextMenuStrip.Items.Add(
                    MainViewModel.GetText("Exit"),
                    WindowsUtils.GetImage(PackIconKind.ExitRun, brush, pen),
                    Exit);
            }

            private void NotifyIconDoubleClicked(object sender, EventArgs e)
                => _window.BringToForeground();

            private void TogglePlayBack(object sender, EventArgs e)
                => MainViewModel.TogglePlayBackCommand.Execute();

            private void StopPlayBack(object sender, EventArgs e)
                => MainViewModel.StopPlayBackCommand.Execute();

            private void PlayNext(object sender, EventArgs e)
                => MainViewModel.NextCommand.Execute();

            private void PlayPrevious(object sender, EventArgs e)
                => MainViewModel.PreviousCommand.Execute();

            private void Exit(object sender, EventArgs e)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
                MainViewModel.CloseAppCommand.Execute();
            }
        }

        private class CustomRenderer : ToolStripProfessionalRenderer
        {
            public CustomRenderer(Color separator, Color background, Color highLight)
                : base(new CustomColors(separator, background, highLight))
            {
            }
        }

        private class CustomColors : ProfessionalColorTable
        {
            private readonly Color _separator;
            private readonly Color _background;
            private readonly Color _highLight;

            public CustomColors(Color separator, Color background, Color highLight)
            {
                _separator = separator;
                _background = background;
                _highLight = highLight;
            }

            //Used
            public override Color SeparatorLight => _separator;
            public override Color SeparatorDark => _separator;
            public override Color ButtonSelectedHighlight => _highLight;
            public override Color MenuBorder => _background;
            public override Color MenuItemBorder => _background;
            public override Color ImageMarginGradientBegin => _background;
            public override Color ImageMarginGradientMiddle => _background;
            public override Color ImageMarginGradientEnd => _background;

            //Not used
            //public override Color MenuItemSelected => Color.Yellow;
            //public override Color MenuItemSelectedGradientBegin => Color.Yellow;
            //public override Color MenuItemSelectedGradientEnd => Color.Yellow;
            //public override Color ButtonSelectedHighlightBorder => Color.Red;
            //public override Color ButtonSelectedGradientEnd => Color.Green;
            //public override Color ButtonSelectedGradientMiddle => Color.Pink;
            //public override Color MenuItemPressedGradientBegin => Color.Yellow;
            //public override Color ButtonCheckedHighlightBorder => Color.Yellow;
            //public override Color ButtonCheckedHighlight => Color.Yellow;
            //public override Color ButtonCheckedGradientBegin => Color.Yellow;
            //public override Color ButtonCheckedGradientEnd => Color.Yellow;
            //public override Color ButtonCheckedGradientMiddle => Color.Yellow;
            //public override Color ButtonPressedBorder => Color.Yellow;
            //public override Color ButtonPressedGradientBegin => Color.Yellow;
            //public override Color ButtonPressedGradientEnd => Color.Yellow;
            //public override Color ButtonPressedGradientMiddle => Color.Yellow;
            //public override Color ButtonPressedHighlight => Color.Yellow;
            //public override Color ButtonPressedHighlightBorder => Color.Yellow;
            //public override Color ButtonSelectedBorder => Color.Yellow;
            //public override Color ButtonSelectedGradientBegin => Color.Yellow;
            //public override Color CheckBackground => Color.Yellow;
            //public override Color CheckPressedBackground => Color.Yellow;
            //public override Color CheckSelectedBackground => Color.Yellow;
            //public override Color GripDark => Color.Yellow;
            //public override Color GripLight => Color.Yellow;
            //public override Color ImageMarginRevealedGradientBegin => Color.Yellow;
            //public override Color ImageMarginRevealedGradientEnd => Color.Yellow;
            //public override Color ImageMarginRevealedGradientMiddle => Color.Yellow;
            //public override Color MenuItemPressedGradientEnd => Color.Yellow;
            //public override Color MenuItemPressedGradientMiddle => Color.Yellow;
            //public override Color MenuStripGradientBegin => Color.Yellow;
            //public override Color MenuStripGradientEnd => Color.Yellow;
            //public override Color OverflowButtonGradientBegin => Color.Yellow;
            //public override Color OverflowButtonGradientEnd => Color.Yellow;
            //public override Color RaftingContainerGradientBegin => Color.Yellow;
            //public override Color OverflowButtonGradientMiddle => Color.Yellow;
            //public override Color RaftingContainerGradientEnd => Color.Yellow;
            //public override Color StatusStripGradientEnd => Color.Yellow;
            //public override Color StatusStripGradientBegin => Color.Yellow;
            //public override Color ToolStripContentPanelGradientBegin => Color.Yellow;
            //public override Color ToolStripContentPanelGradientEnd => Color.Yellow;
            //public override Color ToolStripDropDownBackground => Color.Yellow;
            //public override Color ToolStripGradientBegin => Color.Yellow;
            //public override Color ToolStripGradientEnd => Color.Yellow;
            //public override Color ToolStripGradientMiddle => Color.Yellow;
            //public override Color ToolStripPanelGradientBegin => Color.Yellow;
            //public override Color ToolStripPanelGradientEnd => Color.Yellow;
            //public override Color ToolStripBorder => Color.Brown;
        }
    }
}
