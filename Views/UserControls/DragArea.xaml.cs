using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
