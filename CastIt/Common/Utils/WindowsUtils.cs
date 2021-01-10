using CastIt.Common.Extensions;
using CastIt.Domain.Enums;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace CastIt.Common.Utils
{
    public static class WindowsUtils
    {
        public static T FindAncestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        public static T GetFirstVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        return (T)child;
                    }

                    T childItem = GetFirstVisualChild<T>(child);
                    if (childItem != null)
                    {
                        return childItem;
                    }
                }
            }

            return null;
        }

        public static FrameworkElement GetDescendantFromName(DependencyObject parent, string name)
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);

            if (count < 1)
            {
                return null;
            }

            for (var i = 0; i < count; i++)
            {
                if (VisualTreeHelper.GetChild(parent, i) is FrameworkElement frameworkElement)
                {
                    if (frameworkElement.Name == name)
                    {
                        return frameworkElement;
                    }

                    frameworkElement = GetDescendantFromName(frameworkElement, name);
                    if (frameworkElement != null)
                    {
                        return frameworkElement;
                    }
                }
            }
            return null;
        }

        public static void ChangeTheme(AppThemeType appTheme, string hexAccentColor)
        {
            var baseTheme = appTheme == AppThemeType.Dark ? Theme.Dark : Theme.Light;
            ITheme theme = System.Windows.Application.Current.Resources.GetTheme();

            float lightBy = appTheme == AppThemeType.Dark ? 0.2f : 0.5f;
            float midBy = appTheme == AppThemeType.Dark ? 0.15f : 0.2f;

            var accentColor = hexAccentColor.ToColor();
            var primaryLight = accentColor.LerpLight(lightBy).ToHexString().ToMediaColor();
            var primaryMid = accentColor.LerpLight(midBy).ToHexString().ToMediaColor();
            var primaryDark = hexAccentColor.ToMediaColor();
            var secondary = hexAccentColor.ToMediaColor();

            theme.PrimaryLight = new ColorPair(primaryLight, primaryLight);
            theme.PrimaryMid = new ColorPair(primaryMid, primaryMid);
            theme.PrimaryDark = new ColorPair(primaryDark, primaryDark);

            theme.SecondaryDark =
                theme.SecondaryMid =
                    theme.SecondaryLight = new ColorPair(secondary, secondary);
            theme.SetBaseTheme(baseTheme);
            System.Windows.Application.Current.Resources.SetTheme(theme);

            const string darkTheme = "/XamlResources/DarkTheme.xaml";
            const string lightTheme = "/XamlResources/LightTheme.xaml";

            var uri = new Uri(appTheme == AppThemeType.Dark ? darkTheme : lightTheme, UriKind.RelativeOrAbsolute);
            var uriToRemove = new Uri(appTheme == AppThemeType.Dark ? lightTheme : darkTheme, UriKind.RelativeOrAbsolute);
            var themeDictionary = System.Windows.Application.Current.Resources.MergedDictionaries.Last();

            var currentTheme = themeDictionary.MergedDictionaries
                .FirstOrDefault(r => r.Source == uriToRemove);

            if (!themeDictionary.MergedDictionaries.Any(r => r.Source == uri))
            {
                themeDictionary.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = uri
                });
            }

            if (currentTheme != null)
                themeDictionary.MergedDictionaries.Remove(currentTheme);
        }

        public static void CenterWindow(Window window)
        {
            var currentMonitor = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(window!).Handle);
            CenterWindow(window, currentMonitor);
        }

        public static void CenterWindow(Window w, Screen screen)
        {
            int screenW = screen.Bounds.Width;
            int screenH = screen.Bounds.Height;
            int screenTop = screen.Bounds.Top;
            int screenLeft = screen.Bounds.Left;

            w.Left = PixelsToPoints((int)(screenLeft + (screenW - PointsToPixels(w.Width, "X")) / 2), "X");
            w.Top = PixelsToPoints((int)(screenTop + (screenH - PointsToPixels(w.Height, "Y")) / 2), "Y");
        }

        public static double PixelsToPoints(int pixels, string direction)
        {
            if (direction == "X")
            {
                return pixels * SystemParameters.WorkArea.Width / Screen.PrimaryScreen.WorkingArea.Width;
            }

            return pixels * SystemParameters.WorkArea.Height / Screen.PrimaryScreen.WorkingArea.Height;
        }

        public static double PointsToPixels(double wpfPoints, string direction)
        {
            if (direction == "X")
            {
                return wpfPoints * Screen.PrimaryScreen.WorkingArea.Width / SystemParameters.WorkArea.Width;
            }

            return wpfPoints * Screen.PrimaryScreen.WorkingArea.Height / SystemParameters.WorkArea.Height;
        }
    }
}
