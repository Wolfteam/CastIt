using Microsoft.Xaml.Behaviors;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace CastIt.Common.Behaviours
{
    /// <summary>
    /// https://stackoverflow.com/questions/8088595/synchronizing-multi-select-listbox-with-mvvm
    /// </summary>
    public class MultiSelectionBehavior : Behavior<ListBox>
    {
        private bool _isUpdatingTarget;
        private bool _isUpdatingSource;

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IList), typeof(MultiSelectionBehavior), new UIPropertyMetadata(null, SelectedItemsChanged));

        public IList SelectedItems
        {
            get => (IList)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.Unloaded += OnUnloaded;
            base.OnAttached();
            if (SelectedItems == null)
                return;
            AssociatedObject.SelectedItems.Clear();
            foreach (var item in SelectedItems)
            {
                AssociatedObject.SelectedItems.Add(item);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            OnDetaching();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Unloaded -= OnUnloaded;
            AssociatedObject.SelectionChanged -= ListBoxSelectionChanged;

            if (AssociatedObject.SelectedItems is INotifyCollectionChanged val)
            {
                val.CollectionChanged -= SourceCollectionChanged;
            }
        }

        private static void SelectedItemsChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (!(o is MultiSelectionBehavior behavior))
                return;

            var oldValue = e.OldValue as INotifyCollectionChanged;
            var newValue = e.NewValue as INotifyCollectionChanged;

            if (oldValue != null)
            {
                oldValue.CollectionChanged -= behavior.SourceCollectionChanged;
                behavior.AssociatedObject.SelectionChanged -= behavior.ListBoxSelectionChanged;
            }

            if (newValue == null)
                return;

            behavior.AssociatedObject.SelectedItems.Clear();
            foreach (var item in (IEnumerable)newValue)
            {
                behavior.AssociatedObject.SelectedItems.Add(item);
            }

            behavior.AssociatedObject.SelectionChanged += behavior.ListBoxSelectionChanged;
            newValue.CollectionChanged += behavior.SourceCollectionChanged;
        }

        private void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isUpdatingSource)
                return;

            if (AssociatedObject is null)
            {
                System.Diagnostics.Debug.WriteLine("Collection associated object is null");
                return;
            }

            try
            {
                //TODO: TRY TO FIX THE CRASH WHEN SWITCHING VIEWS
                System.Diagnostics.Debug.WriteLine("Collection changed started....");
                _isUpdatingTarget = true;

                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        AssociatedObject.SelectedItems.Remove(item);
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        AssociatedObject.SelectedItems.Add(item);
                    }
                }

                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    AssociatedObject.SelectedItems.Clear();
                }
            }
            finally
            {
                _isUpdatingTarget = false;
                System.Diagnostics.Debug.WriteLine("Collection changed completed");
            }
        }

        private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingTarget)
                return;

            var selectedItems = this.SelectedItems;
            if (selectedItems == null)
                return;

            try
            {
                _isUpdatingSource = true;
                System.Diagnostics.Debug.WriteLine("Selection changed started....");
                foreach (var item in e.RemovedItems)
                {
                    selectedItems.Remove(item);
                }

                foreach (var item in e.AddedItems)
                {
                    selectedItems.Add(item);
                }
            }
            finally
            {
                _isUpdatingSource = false;
                System.Diagnostics.Debug.WriteLine("Selection changed completed");
            }
        }
    }
}
