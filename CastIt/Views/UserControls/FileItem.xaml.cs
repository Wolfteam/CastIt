using System.Windows.Controls;
using System.Windows.Input;

namespace CastIt.Views.UserControls
{
    public partial class FileItem : UserControl
    {
        public FileItem()
        {
            InitializeComponent();
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            FileItemStackPanel.Children.Add(new Label
            {
                Content = "Options",
                ContextMenu = (ContextMenu)Resources["FileContextMenu"]
            });
        }
    }
}
