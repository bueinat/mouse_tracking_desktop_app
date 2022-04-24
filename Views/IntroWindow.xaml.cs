using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for IntroWindow.xaml
    /// </summary>
    public partial class IntroWindow : Window
    {
        private readonly Dictionary<string, int> buttonToWindow;
        public ViewModels.MainControllerViewModel vm;

        public ViewModels.MainControllerViewModel VM { get; set; }

        public IntroWindow()
        {
            InitializeComponent();
            vm = (Application.Current as App).MainVM;
            DataContext = vm;

            buttonToWindow = new Dictionary<string, int>
            {
                { "upload and watch videos", 0 },
                { "analyze experiments", 1 },
                { "export reports", 2 }
            };
        }

        //private void Button_Connect(object sender, RoutedEventArgs e)
        //{
        //    connecting_button.Content = "connecting...";
        //    vm.Connect();
        //    connecting_button.Content = "connected!";
        //}

        private void Go_to_screen_Click(object sender, RoutedEventArgs e)
        {
            Button clicked = (Button)sender;
            TabsWindow tabsWindow = new TabsWindow();
            tabsWindow.Show();
            tabsWindow.SetTab(buttonToWindow[clicked.Content.ToString()]);
            Close();
        }
    }
}