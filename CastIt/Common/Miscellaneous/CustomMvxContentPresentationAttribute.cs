using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using System;

namespace CastIt.Common.Miscellaneous
{
    public class CustomMvxContentPresentationAttribute : MvxContentPresentationAttribute
    {
        public string ContentFrame { get; set; } = "ContentFrame";
        public bool NoHistory { get; set; }

        public CustomMvxContentPresentationAttribute(Type type, bool stackNavigation = false)
        {
            ViewModelType = type;
            StackNavigation = stackNavigation;
        }
    }
}
