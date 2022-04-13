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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Model = new Models.MainControllerModel();
            MainVM = new ViewModels.MainControllerViewModel(Model);
            VCVM = new ViewModels.VideoControllerViewModel(Model.VC);

            // Create main application window
            Views.MainWindow mainWindow = new Views.MainWindow();
            mainWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Model.StopMethod();
        }
    }
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Timers;
//using MongoDB.Bson;
//using MongoDB.Driver;

//namespace ConsoleApp1
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            string connectionString = "mongodb://127.0.0.1:27017";
//            string databaseName = "test_dbs";
//            string videoName = "Odor28.avi";

//            // connecting to mongodb server
//            MongoClient dbClient = new MongoClient(connectionString);
//            IMongoDatabase database = dbClient.GetDatabase(databaseName);

//            // get collections
//            IMongoCollection<Video> videoCollection = database.GetCollection<Video>("videos");
//            IMongoCollection<Analysis> analysisCollection = database.GetCollection<Analysis>("analysis");

//            // get specific video (by name)
//            Video video = videoCollection.Find(x => x.Name == videoName).FirstOrDefault();
//            // get its matching analysis
//            Analysis analysis = analysisCollection.Find(x => x.ID == video.Analysis).FirstOrDefault();

//        }
//    }
//}