using Microsoft.Win32;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace mouse_tracking_web_app.Views {
    /// <summary>
    /// Interaction logic for VideoView.xaml
    /// </summary>
    public partial class VideoView : UserControl
    {
        public ViewModels.MainControllerViewModel vm;
        private readonly OpenFileDialog uploadVideoDialog;
        public VideoView()
        {
            InitializeComponent();
            //this.DataContext = this;
            vm = (Application.Current as App).MainVM;
            DataContext = vm;
            uploadVideoDialog = new OpenFileDialog
            {
                Filter = "Video Files (*.mp4;*.avi)|*.mp4;*.avi|All files (*.*)|*.*"
            };
        }

        public ViewModels.MainControllerViewModel VM { get; set; }
        private async void ProcessVideoButtonClicked(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1);
            BtnProcessVideo.Content = "Processing...";
            videoName.Text = uploadVideoDialog.FileName;
            BtnProcessVideo.Content = "Process";
        }

        private void UploadVideoButtonClicked(object sender, RoutedEventArgs e)
        {
            if (uploadVideoDialog.ShowDialog() == true)
            {
                videoName1.Text = System.IO.Path.GetFileName(uploadVideoDialog.FileName);
            }
        }
    }
}