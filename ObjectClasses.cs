using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace mouse_tracking_web_app
{
    public class SettingsInstance : INotifyPropertyChanged
    {
        #region CTORs

        public SettingsInstance()
        {
            PythonPath = "";
            ConnectionString = "";
            DatabaseName = "";
            FeaturesList = "";
            FileTypesList = "";
            VideoTypesList = "";
            PlotMarkerSize = double.NaN;
        }

        public static SettingsInstance LoadSavedSettings(string fileName)
        {
            using (StreamReader sw = new StreamReader(fileName))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(SettingsInstance));
                return xmls.Deserialize(sw) as SettingsInstance;
            }
        }

        #endregion CTORs

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsInstance Copy()
        {
            return new SettingsInstance()
            {
                PythonPath = PythonPath,
                ConnectionString = ConnectionString,
                DatabaseName = DatabaseName,
                FeaturesList = FeaturesList,
                FileTypesList = FileTypesList,
                VideoTypesList = VideoTypesList,
                PlotMarkerSize = PlotMarkerSize
            };
        }

        public void Save(string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(SettingsInstance));
                xmls.Serialize(sw, this);
            }
        }

        public void UpdateBasedOnSource(SettingsInstance source)
        {
            if (!string.IsNullOrEmpty(source.PythonPath))
                PythonPath = source.PythonPath;
            if (!string.IsNullOrEmpty(source.ConnectionString))
                ConnectionString = source.ConnectionString;
            if (!string.IsNullOrEmpty(source.DatabaseName))
                DatabaseName = source.DatabaseName;
            if (!string.IsNullOrEmpty(source.FeaturesList))
                FeaturesList = source.FeaturesList;
            if (!string.IsNullOrEmpty(source.FileTypesList))
                FileTypesList = source.FileTypesList;
            if (!string.IsNullOrEmpty(source.VideoTypesList))
                VideoTypesList = source.VideoTypesList;
            if (!double.IsNaN(source.PlotMarkerSize))
                PlotMarkerSize = source.PlotMarkerSize;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region pythonPath

        private string pythonPath;

        public string PythonPath
        {
            get => pythonPath;
            set
            {
                pythonPath = value;
                OnPropertyChanged("PythonPath");
            }
        }

        #endregion pythonPath

        #region connectionString

        private string connectionString;

        public string ConnectionString
        {
            get => connectionString;
            set
            {
                connectionString = value;
                OnPropertyChanged("ConnectionString");
            }
        }

        #endregion connectionString

        #region databaseName

        private string databaseName;

        public string DatabaseName
        {
            get => databaseName;
            set
            {
                databaseName = value;
                OnPropertyChanged("DatabaseName");
            }
        }

        #endregion databaseName

        #region featuresList

        private string featuresList;

        public string FeaturesList
        {
            get => featuresList;
            set
            {
                featuresList = value;
                OnPropertyChanged("FeaturesList");
            }
        }

        #endregion featuresList

        #region fileTypesList

        private string fileTypesList;

        public string FileTypesList
        {
            get => fileTypesList;
            set
            {
                fileTypesList = value;
                OnPropertyChanged("FileTypesList");
            }
        }

        #endregion fileTypesList

        #region videoTypesList

        private string videoTypesList;

        public string VideoTypesList
        {
            get => videoTypesList;
            set
            {
                videoTypesList = value;
                OnPropertyChanged("VideoTypesList");
            }
        }

        #endregion videoTypesList

        #region plotMarkerSize

        private double plotMarkerSize;

        public double PlotMarkerSize
        {
            get => plotMarkerSize;
            set
            {
                plotMarkerSize = value;
                OnPropertyChanged("PlotMarkerSize");
            }
        }

        #endregion plotMarkerSize
    }
}