using CastIt.ViewModels.Items;
using System.Windows;
using System.Windows.Controls;

namespace CastIt.Common.Selectors
{
    public class FileOptionsItemContainerStyleSelector : StyleSelector
    {
        public Style Dynamic { get; set; }
        public Style Normal { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is FileItemOptionsViewModel)
                return Dynamic;
            return Normal;
        }
    }
}
