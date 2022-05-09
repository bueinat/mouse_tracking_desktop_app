using System.Windows;

namespace mouse_tracking_web_app
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Models.MainControllerModel Model { get; internal set; }
        public ViewModels.MainControllerViewModel MainVM { get; internal set; }
        public ViewModels.VideoControllerViewModel VCVM { get; internal set; }
        public ViewModels.PlottingControllerViewModel PCVM { get; internal set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Model = new Models.MainControllerModel();
            MainVM = new ViewModels.MainControllerViewModel(Model);
            VCVM = new ViewModels.VideoControllerViewModel(Model.VC);
            PCVM = new ViewModels.PlottingControllerViewModel(Model.PC);

            // Create main application window
            Views.IntroWindow introWindow = new Views.IntroWindow();
            introWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Model.StopMethod();
        }
    }
}