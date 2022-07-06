using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace mouse_tracking_web_app.ViewModels
{
    public class SettingsManager : INotifyPropertyChanged
    {
        #region settingsFiles

        // TODO: maybe save those somewhere else (like where I saved yolov5 and those stuff)
        private readonly string defaultSettingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
            "\\MouseApp\\Settings\\defaultSettings.xml";

        private readonly string userSettingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
            "\\MouseApp\\Settings\\userSettings.xml";

        #endregion settingsFiles

        #region settingsMethods

        public SettingsManager()
        {
            CurrentSettings = SettingsInstance.LoadSavedSettings(userSettingsPath);
            UpdatableSettings = new SettingsInstance();
        }

        public void ResetToDefaultSettings()
        {
            CurrentSettings = SettingsInstance.LoadSavedSettings(defaultSettingsPath);
            UpdatableSettings = new SettingsInstance();
        }

        public void SaveSettings()
        {
            CurrentSettings.Save(userSettingsPath);
        }

        public void UpdateSettings()
        {
            CurrentSettings.UpdateBasedOnSource(UpdatableSettings);
            UpdatableSettings = new SettingsInstance();
        }

        #endregion settingsMethods

        public event PropertyChangedEventHandler PropertyChanged;

        #region settingsInstances

        private SettingsInstance currentSettings;
        private SettingsInstance updatableSettings;

        public SettingsInstance CurrentSettings
        {
            get => currentSettings;
            set
            {
                currentSettings = value;
                NotifyPropertyChanged("CurrentSettings");
                NotifyPropertyChanged("PythonPath");
                NotifyPropertyChanged("ConnectionString");
                NotifyPropertyChanged("DatabaseName");
                NotifyPropertyChanged("FeaturesList");
                NotifyPropertyChanged("FileTypesList");
                NotifyPropertyChanged("VideoTypesList");
                NotifyPropertyChanged("PlotMarkerSize");
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

        public string PythonPath => CurrentSettings.PythonPath;
        public string ConnectionString => CurrentSettings.ConnectionString;
        public string DatabaseName => CurrentSettings.DatabaseName;
        public List<string> FeaturesList => new List<string>(CurrentSettings.FeaturesList.Split(','));
        public List<string> FileTypesList => new List<string>(CurrentSettings.FullTypesList.Split(','));
        public List<string> VideoTypesList => new List<string>(CurrentSettings.VideoTypesList.Split(','));
        public double PlotMarkerSize => CurrentSettings.PlotMarkerSize;
    }
}