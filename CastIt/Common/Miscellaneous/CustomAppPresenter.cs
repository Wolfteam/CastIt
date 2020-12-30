using CastIt.Common.Utils;
using MvvmCross.Base;
using MvvmCross.Platforms.Wpf.Presenters;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.Presenters;
using MvvmCross.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CastIt.Common.Miscellaneous
{
    public class CustomAppPresenter : MvxWpfViewPresenter
    {
        public CustomAppPresenter(ContentControl root) : base(root)
        {
        }

        public override void RegisterAttributeTypes()
        {
            base.RegisterAttributeTypes();
            AttributeTypesToActionsDictionary.Register<CustomMvxContentPresentationAttribute>(
                (viewType, attribute, request) =>
                {
                    var view = WpfViewLoader.CreateView(request);
                    return ShowContentView(view, (CustomMvxContentPresentationAttribute)attribute, request);
                }, (viewModel, attribute) =>
                {
                    return CloseContentView(viewModel);
                });
        }

        public override async Task<bool> Close(IMvxViewModel toClose)
        {
            // toClose is window
            if (FrameworkElementsDictionary.Any(i => (i.Key as IMvxWpfView)?.ViewModel == toClose) && await CloseWindow(toClose))
                return true;
            //TODO: PROPERLY CLOSE THE SPLASH VIEW
            // toClose is content
            if (FrameworkElementsDictionary.Any(i => i.Value.Any() && (i.Value.Peek() as IMvxWpfView)?.ViewModel == toClose) && await CloseContentView(toClose))
                return true;

            return false;
        }

        protected override Task<bool> ShowContentView(
            FrameworkElement element,
            MvxContentPresentationAttribute attribute,
            MvxViewModelRequest request)
        {
            if (!(attribute is CustomMvxContentPresentationAttribute customMvxViewForAttribute))
                return base.ShowContentView(element, attribute, request);

            var contentControl = FrameworkElementsDictionary.Keys
                .FirstOrDefault(w => (w as MvxWindow)?.Identifier == attribute.WindowIdentifier)
                ?? FrameworkElementsDictionary.Keys.Last();

            if (!customMvxViewForAttribute.NoHistory && !attribute.StackNavigation && FrameworkElementsDictionary[contentControl].Any())
                FrameworkElementsDictionary[contentControl].Pop(); // Close previous view

            //We can't use a frame because we wouldn't be able to access the parents data
            //that's why we use a content control
            if (WindowsUtils.GetDescendantFromName(contentControl, customMvxViewForAttribute.ContentFrame) is ContentControl control)
            {
                control.Content = element;
            }
            else
            {
                contentControl.Content = element;
            }

            if (customMvxViewForAttribute.NoHistory)
                return Task.FromResult(true);

            FrameworkElementsDictionary[contentControl].Push(element);
            return Task.FromResult(true);
        }
    }
}
