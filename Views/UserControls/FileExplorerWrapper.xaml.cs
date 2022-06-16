using System.Windows;
using System.Windows.Controls;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for FileExplorerWrapper.xaml
    /// </summary>
    public partial class FileExplorerWrapper : UserControl
    {
        public FileExplorerWrapper()
        {
            InitializeComponent();
            DataContext = (Application.Current as App).NTVM;
        }
    }
}