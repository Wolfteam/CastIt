using System.Windows.Controls;
using CastIt.Common.Extensions;

namespace CastIt.Views.UserControls
{
    public partial class EditPlayListName : UserControl
    {
        public TextBox PlayListNameTextBox 
            => PlayListName;
        public EditPlayListName()
        {
            InitializeComponent();
        }

        public void FocusTextBox() => PlayListNameTextBox.FocusAndSetPointer();
    }
}
