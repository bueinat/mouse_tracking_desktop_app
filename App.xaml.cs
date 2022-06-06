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
            // create models and view models
            Model = new Models.MainControllerModel();
            MainVM = new ViewModels.MainControllerViewModel(Model);
            VCVM = new ViewModels.VideoControllerViewModel(Model.VC);
            PCVM = new ViewModels.PlottingControllerViewModel(Model.PC);

            ////Disable shutdown when the dialog closes
            //Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            ////var dialog = new DialogWindow();
            //FolderBrowserDialog dialog = new FolderBrowserDialog();

            //    DialogResult result = dialog.ShowDialog();

            //    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            //    {
            //        string[] files = Directory.GetFiles(dialog.SelectedPath);

            //        System.Windows.Forms.MessageBox.Show("Files found: " + files.Length.ToString(), "Message");
            //    }

            //dialog.ShowDialog()

            //if (dialog.ShowDialog() == true)
            //{
            //    Views.IntroWindow introWindow = new Views.IntroWindow(); //dialog.Data);
            //    //Re-enable normal shutdown mode.
            //    Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
            //    introWindow.Show();
            //}
            //else
            //{
            //    MessageBox.Show("Unable to load data.", "Error", MessageBoxButton.OK);
            //    Current.Shutdown(-1);
            //}

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