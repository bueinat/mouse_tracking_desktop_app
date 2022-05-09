using System.Windows;
using System.Windows.Controls;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for VideoController.xaml
    /// </summary>
    public partial class VideoController : UserControl
    {
        public VideoController()
        {
            InitializeComponent();
            DataContext = (Application.Current as App).VCVM;
        }

        private void FPanelButtonClicked(object sender, RoutedEventArgs e)
        {
            ShowHideButton.Content = ShowHideButton.Content.ToString().StartsWith("Open") ? "Close Features Panel" : "Open Features Panel";
        }

        private void PlayPauseButtonClicked(object sender, RoutedEventArgs e)
        {
            PlayPauseButton.Content = !(bool)PlayPauseButton.Content;
            if (VideoTimeSlider.Value == VideoTimeSlider.Maximum)
            {
                VideoTimeSlider.Value = 0;
                PlayPauseButton.Content = !(bool)PlayPauseButton.Content;
            }
        }
    }
}