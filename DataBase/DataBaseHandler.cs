using MongoDB.Bson;
using MongoDB.Driver;
using System.ComponentModel;

namespace mouse_tracking_web_app.DataBase
{
    public class DataBaseHandler : INotifyPropertyChanged
    {
        // TODO: move those to appsetting file
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

        public Analysis GetAnalysisByVideo(string videoID)
        {
            ObjectId id = ObjectId.Parse(videoID);
            Video video = videoCollection.Find(x => x.ID == id).FirstOrDefault();
            return analysisCollection.Find(x => x.ID == video.Analysis).FirstOrDefault();
        }
    }
}