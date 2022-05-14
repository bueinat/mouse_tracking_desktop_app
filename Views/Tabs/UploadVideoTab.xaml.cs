using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for UploadVideoTab.xaml
    /// </summary>
    public partial class UploadVideoTab : UserControl
    {
        private readonly OpenFileDialog uploadVideoDialog;

        public UploadVideoTab()
        {
            InitializeComponent();
            DataContext = (Application.Current as App).MainVM;
            uploadVideoDialog = new OpenFileDialog
            {
                Filter = "Video Files (*.mp4; *.avi)|*.mp4;*.avi|All files (*.*)|*.*"
            };
        }

        private void ProcessVideoButtonClicked(object sender, RoutedEventArgs e)
        {
            videoName.Text = uploadVideoDialog.FileName;
        }

        private void UploadVideoButtonClicked(object sender, RoutedEventArgs e)
        {
            if (uploadVideoDialog.ShowDialog() == true)
                videoNameNoPath.Text = System.IO.Path.GetFileName(uploadVideoDialog.FileName);
        }
    }
}