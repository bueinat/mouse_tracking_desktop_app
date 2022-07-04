using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace mouse_tracking_web_app.ViewModels
{
    public class SettingsInstance : INotifyPropertyChanged
    {
        public SettingsInstance() : this("empty")
        {
        }

        public SettingsInstance(string type)
        {
            if (type == "default")
            {
                PythonPath = ConfigurationManager.AppSettings.Get("PythonPath");
                ConnectionString = ConfigurationManager.AppSettings.Get("ConnectionString");
                DatabaseName = ConfigurationManager.AppSettings.Get("DatabaseName");
                FeaturesList = ConfigurationManager.AppSettings.Get("FeaturesList");
                FileTypesList = ConfigurationManager.AppSettings.Get("FileTypesList");
                VideoTypesList = ConfigurationManager.AppSettings.Get("VideoTypesList");
                PlotMarkerSize = double.Parse(ConfigurationManager.AppSettings.Get("PlotMarkerSize"));
            }
            else
            {
                PythonPath = "";
                ConnectionString = "";
                DatabaseName = "";
                FeaturesList = "";
                FileTypesList = "";
                VideoTypesList = "";
                PlotMarkerSize = double.NaN;
            }
        }

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

        public SettingsInstance CopyExistingValues()
        {
            SettingsInstance copy = new SettingsInstance("default");
            if (!string.IsNullOrEmpty(PythonPath))
                copy.PythonPath = PythonPath;
            if (!string.IsNullOrEmpty(ConnectionString))
                copy.ConnectionString = ConnectionString;
            if (!string.IsNullOrEmpty(DatabaseName))
                copy.DatabaseName = DatabaseName;
            if (!string.IsNullOrEmpty(FeaturesList))
                copy.FeaturesList = FeaturesList;
            if (!string.IsNullOrEmpty(FileTypesList))
                copy.FileTypesList = FileTypesList;
            if (!string.IsNullOrEmpty(VideoTypesList))
                copy.VideoTypesList = VideoTypesList;
            if (!double.IsNaN(PlotMarkerSize))
                copy.PlotMarkerSize = PlotMarkerSize;
            return copy;
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

    public class SettingsManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region settingsInstances

        private SettingsInstance currentSettings = new SettingsInstance("default");
        private SettingsInstance updatableSettings = new SettingsInstance();

        public SettingsInstance CurrentSettings
        {
            get => currentSettings;
            set
            {
                currentSettings = value;
                NotifyPropertyChanged("CurrentSettings");
            }
        }

        public SettingsInstance UpdatableSettings
        {
            get => updatableSettings;
            set
            {
                updatableSettings = value;
                NotifyPropertyChanged("UpdatableSettings");
            }
        }

        #endregion settingsInstances

        public string FirstCharToUpperCase(string str)
        {
            return !string.IsNullOrEmpty(str) && char.IsLower(str[0])
                ? str.Length == 1 ? char.ToUpper(str[0]).ToString() : char.ToUpper(str[0]) + str.Substring(1)
                : str;
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ResetToDefaultSettings()
        {
            CurrentSettings = new SettingsInstance("default");
            UpdatableSettings = new SettingsInstance();
        }

        public void UpdateSettings()
        {
            CurrentSettings = UpdatableSettings.CopyExistingValues();
            UpdatableSettings = new SettingsInstance();
        }
    }
}