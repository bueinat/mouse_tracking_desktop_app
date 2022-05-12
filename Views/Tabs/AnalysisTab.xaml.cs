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
            RadioButton rButton = sender as RadioButton;
            if ((rButton.GroupName == "color") && !(ColorParameterName is null))
                ColorParameterName.Text = (string)(sender as RadioButton).Content;
            if ((rButton.GroupName == "size") && !(SizeParameterName is null))
                SizeParameterName.Text = (string)(sender as RadioButton).Content;
        }
    }
}