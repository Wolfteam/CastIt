using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CastIt.Views.UserControls
{
    /// <summary>
    /// Interaction logic for FileItem.xaml
    /// </summary>
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
