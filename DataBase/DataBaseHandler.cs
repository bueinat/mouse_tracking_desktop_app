using MongoDB.Driver;
using System;
using System.ComponentModel;

namespace mouse_tracking_web_app.DataBase
{
    public class DataBaseHandler : INotifyPropertyChanged
    {
        private readonly string connectionString = "mongodb://127.0.0.1:27017";
        private readonly string databaseName = "test_dbs";

        private readonly Models.MainControllerModel model;
        private IMongoCollection<Analysis> analysisCollection;
        private MongoClient client;

        private bool isConnected = false;
        private IMongoCollection<Video> videoCollection;
        public DataBaseHandler(Models.MainControllerModel model)
        {
            this.model = model;
            model.PropertyChanged +=
            delegate (Object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("DBH_" + e.PropertyName);
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Connect()
        {
            if (!isConnected)
            {
                client = new MongoClient(connectionString);
                IMongoDatabase database = client.GetDatabase(databaseName);

                videoCollection = database.GetCollection<Video>("videos");
                analysisCollection = database.GetCollection<Analysis>("analysis");
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}