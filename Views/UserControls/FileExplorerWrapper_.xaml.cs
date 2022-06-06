using System.Windows;
using System.Windows.Controls;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for FileExplorerWrapper_.xaml
    /// </summary>
    public partial class FileExplorerWrapper_ : Page
    {
        public FileExplorerWrapper_()
        {
            InitializeComponent();
            DataContext = (Application.Current as App).NTVM;
        }
    }
}
