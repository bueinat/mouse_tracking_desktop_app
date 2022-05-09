using MongoDB.Bson;
using MongoDB.Driver;
using System.ComponentModel;
using System.Configuration;

namespace mouse_tracking_web_app.DataBase
{
    public class DataBaseHandler : INotifyPropertyChanged
    {
        //private readonly Models.MainControllerModel model;
        private IMongoCollection<Analysis> analysisCollection;

        private MongoClient client;
        //public Dictionary<string, List<KeyValuePair<int, int>>> FeaturesTimes;

        private bool isConnected = false;
        private IMongoCollection<Video> videoCollection;

        //private readonly List<string> featuresNames = new List<string>
        //    {
        //        "IsDrinking",
        //        "IsNoseCasting",
        //        "IsSniffing"
        //    };

        public DataBaseHandler(Models.MainControllerModel model)
        {
            //this.model = model;
            model.PropertyChanged +=
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
                client = new MongoClient(ConfigurationManager.AppSettings.Get("ConnectionString"));
                IMongoDatabase database = client.GetDatabase(ConfigurationManager.AppSettings.Get("DatabaseName"));

                videoCollection = database.GetCollection<Video>("videos");
                analysisCollection = database.GetCollection<Analysis>("analysis");
                //FillFeaturesTimes();
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Video GetVideoByID(string videoID)
        {
            ObjectId id = ObjectId.Parse(videoID);
            Video video = videoCollection.Find(x => x.ID == id).FirstOrDefault();
            return video;
        }

        public Analysis GetAnalysisByID(ObjectId analysis)
        {
            return analysisCollection.Find(x => x.ID == analysis).FirstOrDefault();
        }
    }
}