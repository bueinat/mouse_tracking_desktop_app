using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
    }
}