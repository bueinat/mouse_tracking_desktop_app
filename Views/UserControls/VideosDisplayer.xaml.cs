using mouse_tracking_web_app.DataBase;
using System.Collections.Generic;
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
            DataContext = (Application.Current as App).MainVM;
        }

        private void ListBoxItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // treat only double click
            if (e.ClickCount > 1)
            {
                DisplayableVideo video = ((Grid)sender).DataContext as DisplayableVideo;
                if (video.ProcessingState == DisplayableVideo.State.Successful)
                    lbTodoList.SelectedItem = video;
            }
            e.Handled = true;

        }
    }
}