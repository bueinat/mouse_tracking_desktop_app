﻿using System.Windows;

namespace mouse_tracking_web_app
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ViewModels.SettingsManager SM { get; internal set; }
        public Models.MainControllerModel Model { get; internal set; }
        public ViewModels.MainControllerViewModel MainVM { get; internal set; }
        public ViewModels.VideoControllerViewModel VCVM { get; internal set; }
        public ViewModels.PlottingControllerViewModel PCVM { get; internal set; }
        public ViewModels.NavigationTreeViewModel NTVM { get; internal set; }
        public ViewModels.VideoProcessingManager VPM { get; internal set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // create models and view models
            SM = new ViewModels.SettingsManager();
            Model = new Models.MainControllerModel(SM);
            NTVM = new ViewModels.NavigationTreeViewModel(Model, SM);
            MainVM = new ViewModels.MainControllerViewModel(Model, NTVM);
            VCVM = new ViewModels.VideoControllerViewModel(Model.VC);
            PCVM = new ViewModels.PlottingControllerViewModel(Model.PC);
            VPM = new ViewModels.VideoProcessingManager(Model);

            Views.IntroWindow introWindow = new Views.IntroWindow();
            introWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Model.StopMethod();
            SM.SaveSettings();
        }
    }
}