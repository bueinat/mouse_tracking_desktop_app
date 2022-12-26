using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace mouse_tracking_web_app.UtilTypes
{
    public class SettingsInstance : INotifyPropertyChanged
    {
        #region CTORs

        public SettingsInstance() : this(false)
        {
        }

        private SettingsInstance(bool useDef)
        {
            if (useDef)
            {
                PythonPath = ConfigurationManager.AppSettings.Get("PythonPath");
                WorkingPath = ConfigurationManager.AppSettings.Get("WorkingPath");
                DEPath = ConfigurationManager.AppSettings.Get("DEPath");
                ConnectionString = ConfigurationManager.AppSettings.Get("ConnectionString");
                DatabaseName = ConfigurationManager.AppSettings.Get("DatabaseName");
                FeaturesList = ConfigurationManager.AppSettings.Get("FeaturesList");
                FileTypesList = ConfigurationManager.AppSettings.Get("FileTypesList");
                VideoTypesList = ConfigurationManager.AppSettings.Get("VideoTypesList");
                OverrideDB = bool.Parse(ConfigurationManager.AppSettings.Get("OverrideDB"));
                PlotMarkerSize = double.Parse(ConfigurationManager.AppSettings.Get("PlotMarkerSize"));
            }
            else
            {
                PythonPath = "";
                WorkingPath = "";
                DEPath = "";
                ConnectionString = "";
                DatabaseName = "";
                FeaturesList = "";
                FileTypesList = "";
                VideoTypesList = "";
                OverrideDB = false;
                PlotMarkerSize = double.NaN;
            }
        }

        #endregion CTORs

        #region Builders

        public static SettingsInstance LoadSavedSettings(string fileName)
        {
            using (StreamReader sw = new StreamReader(fileName))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(SettingsInstance));
                try
                {
                    return xmls.Deserialize(sw) as SettingsInstance;
                }
                catch (System.InvalidOperationException)
                {
                    if (fileName.Contains("userSettings"))
                        return LoadSavedSettings(fileName.Replace("user", "default"));
                    else if (fileName.Contains("defaultSettings"))
                        return new SettingsInstance(true);
                    else
                        throw new InvalidDataException("The settings file doesn't have a valid name.");
                }
            }
        }

        public SettingsInstance Copy()
        {
            return new SettingsInstance()
            {
                PythonPath = PythonPath,
                WorkingPath = WorkingPath,
                DEPath = DEPath,
                ConnectionString = ConnectionString,
                DatabaseName = DatabaseName,
                FeaturesList = FeaturesList,
                FileTypesList = FileTypesList,
                VideoTypesList = VideoTypesList,
                PlotMarkerSize = PlotMarkerSize,
                OverrideDB = OverrideDB
            };
        }

        #endregion Builders

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
            OverrideDB = source.OverrideDB;
            if (!string.IsNullOrEmpty(source.PythonPath))
                PythonPath = source.PythonPath;
            if (!string.IsNullOrEmpty(source.WorkingPath))
                WorkingPath = source.WorkingPath;
            if (!string.IsNullOrEmpty(source.DEPath))
                DEPath = source.DEPath;
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

        #region propertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion propertyChanged

        #region overrideDB

        private bool overrideDB;

        public bool OverrideDB
        {
            get => overrideDB;
            set
            {
                overrideDB = value;
                OnPropertyChanged("OverrideDB");
            }
        }

        #endregion overrideDB

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

        #region workingPath

        private string workingPath;

        public string WorkingPath
        {
            get => workingPath;
            set
            {
                workingPath = value;
                OnPropertyChanged("WorkingPath");
            }
        }

        #endregion workingPath

        #region dePath

        private string dePath;

        public string DEPath
        {
            get => dePath;
            set
            {
                dePath = value;
                OnPropertyChanged("DEPath");
            }
        }

        #endregion dePath

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

        public string FullTypesList => $"{FileTypesList},{VideoTypesList}";

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