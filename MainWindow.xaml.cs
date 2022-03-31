﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mouse_tracking_web_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public ViewModels.MainControllerViewModel vm;

        public ViewModels.MainControllerViewModel VM { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            vm = (Application.Current as App).MainVM;
            DataContext = vm;
        }

        private void Button_Connect(object sender, RoutedEventArgs e)
        {
            connecting_button.Content = "connecting...";
            vm.Connect();
            connecting_button.Content = "connected!";
        }
    }
}
