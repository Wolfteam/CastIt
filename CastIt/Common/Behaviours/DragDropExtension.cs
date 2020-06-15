using CastIt.Common.Utils;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CastIt.Common.Behaviours
{
    /// <summary>
    /// Provides extended support for drag drop operation
    /// https://stackoverflow.com/questions/1316251/wpf-listbox-auto-scroll-while-dragging
    /// </summary>
    public static class DragDropExtension
    {
        public static readonly DependencyProperty ScrollOnDragDropProperty =
        DependencyProperty.RegisterAttached("ScrollOnDragDrop",
            typeof(bool),
            typeof(DragDropExtension),
            new PropertyMetadata(false, HandleScrollOnDragDropChanged));

        public static bool GetScrollOnDragDrop(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (bool)element.GetValue(ScrollOnDragDropProperty);
        }

        public static void SetScrollOnDragDrop(DependencyObject element, bool value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(ScrollOnDragDropProperty, value);
        }

        private static void HandleScrollOnDragDropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement container = d as FrameworkElement;

            if (d == null)
            {
                return;
            }

            Unsubscribe(container);

            if (true.Equals(e.NewValue))
            {
                Subscribe(container);
            }
        }

        private static void Subscribe(FrameworkElement container)
        {
            container.PreviewDragOver += OnContainerPreviewDragOver;
        }

        private static void OnContainerPreviewDragOver(object sender, DragEventArgs e)
        {
            if (!(sender is FrameworkElement container))
            {
                return;
            }

            var scrollViewer = WindowsUtils.GetFirstVisualChild<ScrollViewer>(container);

            if (scrollViewer == null)
            {
                return;
            }

            const double tolerance = 30;
            const double offset = 3;
            double verticalPos = e.GetPosition(container).Y;
            if (verticalPos < tolerance) // Top of visible list? 
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offset); //Scroll up. 
            }
            else if (verticalPos > container.ActualHeight - tolerance) //Bottom of visible list? 
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset); //Scroll down.     
            }
        }

        private static void Unsubscribe(FrameworkElement container)
        {
            container.PreviewDragOver -= OnContainerPreviewDragOver;
        }
    }
}
