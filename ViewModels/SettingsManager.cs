using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.ViewModels
{

    public class SettingsInstance
    {
        public string PythonPath { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string FeaturesList { get; set; }
        public string FileTypesList { get; set; }
        public string VideoTypesList { get; set; }
        public double PlotMarkerSize { get; set; }
    }
    public class SettingsManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Dictionary<string, string> defaultValues = new Dictionary<string, string>()
        {
            ["PythonPath"] = ConfigurationManager.AppSettings.Get("PythonPath"),
            ["ConnectionString"] = ConfigurationManager.AppSettings.Get("ConnectionString"),
            ["DatabaseName"] = ConfigurationManager.AppSettings.Get("DatabaseName"),
            ["FeaturesList"] = ConfigurationManager.AppSettings.Get("FeaturesList"),
            ["FileTypesList"] = ConfigurationManager.AppSettings.Get("FileTypesList"),
            ["VideoTypesList"] = ConfigurationManager.AppSettings.Get("VideoTypesList"),
            ["PlotMarkerSize"] = ConfigurationManager.AppSettings.Get("PlotMarkerSize")
        };

        public SettingsManager()
        {
            ResetToDefaultSettings();
        }
        public void ResetToDefaultSettings()
        {
            PythonPath = defaultValues["PythonPath"];
            ConnectionString = defaultValues["ConnectionString"];
            DatabaseName = defaultValues["DatabaseName"];
            FeaturesList = defaultValues["FeaturesList"];
            FileTypesList = defaultValues["FileTypesList"];
            VideoTypesList = defaultValues["VideoTypesList"];
            PlotMarkerSize = double.Parse(defaultValues["PlotMarkerSize"]);
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string FirstCharToUpperCase(string str)
        {
            return !string.IsNullOrEmpty(str) && char.IsLower(str[0])
                ? str.Length == 1 ? char.ToUpper(str[0]).ToString() : char.ToUpper(str[0]) + str.Substring(1)
                : str;
        }

        public void UpdateFields(Dictionary<string, string> newSettings)
        {
            foreach (KeyValuePair<string, string> pair in newSettings)
            {
                if (!string.IsNullOrEmpty(pair.Value))
                {
                    if (pair.Key == "plotMarkerSize")
                        PlotMarkerSize = double.Parse(pair.Value);
                    else
                    GetType().GetProperty(FirstCharToUpperCase(pair.Key)).SetValue(this, pair.Value);
                }
            }
        }

        private Dictionary<string, string> updatableSettings = new Dictionary<string, string>()
        {
            ["pythonPath"] = "",
            ["connectionString"] = "",
            ["databaseName"] = "",
            ["featuresList"] = "",
            ["fileTypesList"] = "",
            ["videoTypesList"] = "",
            ["plotMarkerSize"] = ""
        };

        //public Dictionary<string, string> 

        #region pythonPath
        private string pythonPath;
        public string PythonPath
        {
            get => pythonPath;
            set
            {
                pythonPath = value;
                NotifyPropertyChanged("PythonPath");
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
                NotifyPropertyChanged("ConnectionString");
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
                NotifyPropertyChanged("DatabaseName");
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
                NotifyPropertyChanged("FeaturesList");
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
                NotifyPropertyChanged("FileTypesList");
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
                NotifyPropertyChanged("VideoTypesList");
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
                NotifyPropertyChanged("PlotMarkerSize");
            }
        }
        #endregion plotMarkerSize

    }
}
