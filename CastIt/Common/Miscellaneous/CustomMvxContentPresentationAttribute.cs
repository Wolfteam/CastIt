using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using System;

namespace CastIt.Common.Miscellaneous
{
    public class CustomMvxContentPresentationAttribute : MvxContentPresentationAttribute
    {
        public string ContentFrame { get; set; } = "ContentFrame";

        public CustomMvxContentPresentationAttribute(Type type)
        {
            ViewModelType = type;
            StackNavigation = false;
        }
    }
}
