using System.Windows.Controls;

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
    }
}
