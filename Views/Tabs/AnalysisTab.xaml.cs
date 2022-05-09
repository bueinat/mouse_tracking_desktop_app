using System.Windows;
using System.Windows.Controls;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for AnalysisTab.xaml
    /// </summary>
    public partial class AnalysisTab : UserControl
    {
        public AnalysisTab()
        {
            InitializeComponent();
            DataContext = (Application.Current as App).PCVM;
        }

        private void HandleCheck(object sender, RoutedEventArgs e)
        {
            if (!(ParameterName is null))
                ParameterName.Text = (string)(sender as RadioButton).Content;
        }
    }
}