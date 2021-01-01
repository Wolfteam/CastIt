using CastIt.ViewModels.Items;
using System.Windows;
using System.Windows.Controls;

namespace CastIt.Common.Selectors
{
    public class PlayListItemCardSelector : DataTemplateSelector
    {
        public DataTemplate Card { get; set; }
        public DataTemplate AddCard { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is PlayListItemViewModel)
                return Card;

            return AddCard;
        }
    }
}
