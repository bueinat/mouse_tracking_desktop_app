using MongoDB.Bson;
using MongoDB.Driver;
using System.ComponentModel;

namespace mouse_tracking_web_app.DataBase
{
    public class DataBaseHandler : INotifyPropertyChanged
    {
        public ViewModels.SettingsManager SM;

        //private readonly Models.MainControllerModel model;
        private IMongoCollection<Analysis> analysisCollection;

        private MongoClient client;
        private IMongoDatabase database;

        private bool isConnected = false;
        private IMongoCollection<Video> videoCollection;

        //private readonly List<string> featuresNames = new List<string>
        //    {
        //        "IsDrinking",
        //        "IsNoseCasting",
        //        "IsSniffing"
        //    };
        public DataBaseHandler(Models.MainControllerModel model, ViewModels.SettingsManager sManager)
        {
            //this.model = model;
            //model.PropertyChanged +=
            //    delegate (object sender, PropertyChangedEventArgs e)
            //    {
            //        NotifyPropertyChanged("DBH_" + e.PropertyName);
            //    };

            SM = sManager;
            sManager.PropertyChanged +=
                delegate (object sender, PropertyChangedEventArgs e)
                {
                    NotifyPropertyChanged("DBH_" + e.PropertyName);
                };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Connect()
        {
            if (!isConnected)
            {
                isConnected = true;
                client = new MongoClient(SM.ConnectionString);
                GetDataBase();
            }
        }

        public void GetDataBase()
        {
            database = client.GetDatabase(SM.DatabaseName);
            videoCollection = database.GetCollection<Video>("videos");
            analysisCollection = database.GetCollection<Analysis>("analysis");
        }

        public bool DoesIDexist(string id, string collectionName)
        {
            return DoesIDexist(ObjectId.Parse(id), collectionName);
        }

        public bool DoesIDexist(ObjectId id, string collectionName)
        {
            if (collectionName == "video")
                return videoCollection.Find(x => x.ID == id).CountDocuments() > 0;
            return collectionName == "analysis"
                ? analysisCollection.Find(x => x.ID == id).CountDocuments() > 0
                : throw new InvalidEnumArgumentException("collection name must be one of [video, analysis]");
        }

        public Analysis GetAnalysisByID(ObjectId analysis)
        {
            return analysisCollection.Find(x => x.ID == analysis).FirstOrDefault();
        }

        public Video GetVideoByID(string videoID)
        {
            ObjectId id = ObjectId.Parse(videoID);
            return videoCollection.Find(x => x.ID == id).First();
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == "ConnectionString")
            {
                isConnected = false;
                Connect();
            }
            if (propertyName == "DatabaseName")
                GetDataBase();
        }
    }
}