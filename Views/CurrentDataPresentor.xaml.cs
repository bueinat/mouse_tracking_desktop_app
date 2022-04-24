using System.Windows;
using System.Windows.Controls;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for CurrentDataPresentor.xaml
    /// </summary>
    public partial class CurrentDataPresentor : UserControl
    {
        private readonly ViewModels.VideoControllerViewModel vm;

        public CurrentDataPresentor()
        {
            vm = (Application.Current as App).VCVM;
            InitializeComponent();
            DataContext = vm;
        }
    }
}