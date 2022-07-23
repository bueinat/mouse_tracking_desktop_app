using mouse_tracking_web_app.DataBase;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for VideosDisplayer.xaml
    /// </summary>
    public partial class VideosDisplayer : UserControl
    {
        public VideosDisplayer()
        {
            InitializeComponent();
            DataContext = (Application.Current as App).VPM;
        }

        private void ListBoxItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // treat only double click
            if ((e.ClickCount > 1) && (e.ChangedButton == MouseButton.Left))
            {
                DisplayableVideo video = ((Grid)sender).DataContext as DisplayableVideo;
                if (video.ProcessingState == DisplayableVideo.State.Successful)
                    VideosListBox.SelectedItem = video;
            }
            e.Handled = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //System.Console.WriteLine("item clicked");
            DisplayableVideo video = ((Grid)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).DataContext as DisplayableVideo;
            video.Stop();
        }
    }
}