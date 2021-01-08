using CastIt.Common.Extensions;
using System.Windows.Controls;

namespace CastIt.Views.Popups
{
    public partial class RenamePlayList : UserControl
    {
        public TextBox PlayListNameTextBox
            => PlayListName;
        public RenamePlayList()
        {
            InitializeComponent();
        }

        public void FocusTextBox() => PlayListNameTextBox.FocusAndSetPointer();
    }
}
