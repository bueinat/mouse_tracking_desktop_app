﻿using System.Windows;
using System.Windows.Controls;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for VideoController.xaml
    /// </summary>
    public partial class VideoController : UserControl
    {
        private readonly ViewModels.VideoControllerViewModel vm;

        public VideoController()
        {
            vm = (Application.Current as App).VCVM;
            InitializeComponent();
            DataContext = vm;
        }
    }
}
