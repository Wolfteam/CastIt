using System.Windows.Controls;

namespace CastIt.Views.UserControls
{
    public partial class AddFromUrl : UserControl
    {
        public TextBox UrlText
            => UrlTextBox;

        public AddFromUrl()
        {
            InitializeComponent();
        }
    }
}
