using mouse_tracking_web_app.ViewModels;
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
using System.Windows.Shapes;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        public SettingsDialog()
        {
            InitializeComponent();
            DataContext = (Application.Current as App).SM;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            //txtAnswer.SelectAll();
            //txtAnswer.Focus();
        }

        private Dictionary<string, string> newSettings = new Dictionary<string, string>()
        {
            ["pythonPath"] = "",
            ["connectionString"] = "",
            ["databaseName"] = "",
            ["featuresList"] = "",
            ["fileTypesList"] = "",
            ["videoTypesList"] = "",
            ["plotMarkerSize"] = ""
        };

        private void DialogOk_Click(object sender, RoutedEventArgs e)
        {
            foreach (object child in baseGrid.Children)
            {
                if (!(child as TextBox is null))
                    newSettings[(child as TextBox).Name] = (child as TextBox).Text;
            }
            (DataContext as SettingsManager).UpdateFields(newSettings);
            DialogResult = true;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as SettingsManager).ResetToDefaultSettings();
        }
    }
}
