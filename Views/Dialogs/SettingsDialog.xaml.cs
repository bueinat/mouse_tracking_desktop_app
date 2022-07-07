using mouse_tracking_web_app.ViewModels;
using System;
using System.Windows;

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

        private void DialogOk_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as SettingsManager).UpdateSettings();
            DialogResult = true;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as SettingsManager).ResetToDefaultSettings();
        }
    }
}