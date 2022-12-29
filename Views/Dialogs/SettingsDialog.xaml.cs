using Microsoft.WindowsAPICodePack.Dialogs;
using mouse_tracking_web_app.ViewModels;
using System.Linq;
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

        private void WP_Button_Click(object sender, RoutedEventArgs e)
        {
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            if (dialog.ShowDialog(window) == CommonFileDialogResult.Ok)
                workingPath.Text = dialog.FileName;
        }

        private void DEP_Button_Click(object sender, RoutedEventArgs e)
        {
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            if (dialog.ShowDialog(window) == CommonFileDialogResult.Ok)
                dePath.Text = dialog.FileName;
        }
    }
}