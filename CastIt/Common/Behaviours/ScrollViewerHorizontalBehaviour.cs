using CastIt.Common.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CastIt.Common.Behaviours
{
    /// <summary>
    /// https://stackoverflow.com/questions/3727439/how-to-enable-horizontal-scrolling-with-mouse
    /// </summary>
    public static class ScrollViewerHorizontalBehaviour
    {
        public static readonly DependencyProperty ShiftWheelScrollsHorizontallyProperty
            = DependencyProperty.RegisterAttached(
                "ShiftWheelScrollsHorizontally",
                typeof(bool),
                typeof(ScrollViewerHorizontalBehaviour),
                new PropertyMetadata(false, UseHorizontalScrollingChangedCallback));

        public static readonly DependencyProperty EnableShiftKeyProperty
            = DependencyProperty.RegisterAttached(
                "EnableShiftKey",
                typeof(bool),
                typeof(ScrollViewerHorizontalBehaviour),
                new PropertyMetadata(true));

        private static void UseHorizontalScrollingChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as UIElement;

            if (element == null)
                throw new Exception("Attached property must be used with UIElement.");

            if ((bool)e.NewValue)
                element.PreviewMouseWheel += OnPreviewMouseWheel;
            else
                element.PreviewMouseWheel -= OnPreviewMouseWheel;
        }

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs args)
        {
            var scrollViewer = WindowsUtils.GetFirstVisualChild<ScrollViewer>((UIElement)sender);

            if (scrollViewer == null)
            {
                if (sender is ScrollViewer)
                {
                    scrollViewer = sender as ScrollViewer;
                }
                else
                {
                    return;
                }
            }

            bool isShiftKeyEnabled = GetEnableShiftKey(scrollViewer);
            if (isShiftKeyEnabled && Keyboard.Modifiers != ModifierKeys.Shift)
                return;

            if (args.Delta < 0)
                scrollViewer.LineRight();
            else
                scrollViewer.LineLeft();

            args.Handled = true;
        }

        public static bool GetEnableShiftKey(ScrollViewer element)
            => (bool)element.GetValue(EnableShiftKeyProperty);
        public static void SetEnableShiftKey(ScrollViewer element, bool value)
            => element.SetValue(EnableShiftKeyProperty, value);

        public static void SetShiftWheelScrollsHorizontally(ItemsControl element, bool value)
            => element.SetValue(ShiftWheelScrollsHorizontallyProperty, value);
        public static bool GetShiftWheelScrollsHorizontally(ItemsControl element)
            => (bool)element.GetValue(ShiftWheelScrollsHorizontallyProperty);
    }
}
