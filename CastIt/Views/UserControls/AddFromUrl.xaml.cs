using System.Windows;
using System.Windows.Controls;
using CastIt.Common.Extensions;

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

        public void FocusUrl()
        {
            UrlText.Text = Clipboard.GetText();
            UrlText.FocusAndSetPointer();
        }
    }
}
