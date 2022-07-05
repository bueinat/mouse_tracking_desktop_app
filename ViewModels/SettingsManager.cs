using System;
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
        private SettingsInstance defaultSettings;
        private SettingsInstance updatableSettings;
        private SettingsInstance userSettings;
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
    }
}