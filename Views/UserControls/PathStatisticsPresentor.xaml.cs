using System.Windows;
using System.Windows.Controls;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for PathStatisticsPresentor.xaml
    /// </summary>
    ///
    // TODO:
    // * fill all nans, since they ruin stuff
    // * reshape the plot
    // * find a way to mention features. Maybe different shapes?
    public partial class PathStatisticsPresentor : UserControl
    {
        public PathStatisticsPresentor()
        {
            InitializeComponent();
            DataContext = (Application.Current as App).PCVM;
        }
    }
}