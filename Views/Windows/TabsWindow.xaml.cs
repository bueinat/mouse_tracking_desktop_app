using System.Windows;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for TabsWindow.xaml
    /// </summary>
    public partial class TabsWindow : Window
    {
        public TabsWindow()
        {
            InitializeComponent();
        }

        internal void SetTab(int index)
        {
            tabs.SelectedIndex = index;
        }
    }
}