using System.Windows;
using System.Windows.Controls;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for DragArea.xaml
    /// </summary>
    public partial class DragArea : UserControl
    {
        //private readonly
        public DragArea()
        {
            InitializeComponent();
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if (data is string fileName)
                (DataContext as ViewModels.MainControllerViewModel).VM_VideoName = fileName;
        }
    }
}