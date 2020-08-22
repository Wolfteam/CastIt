using System.Windows;
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

        public void FocusUrl()
        {
            UrlText.Text = Clipboard.GetText();
            UrlText.Focus();
            if (!string.IsNullOrEmpty(UrlText.Text))
            {
                UrlText.SelectionStart = UrlText.Text.Length;
            }
        }
    }
}
