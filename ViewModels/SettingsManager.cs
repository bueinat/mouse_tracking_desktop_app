﻿using mouse_tracking_web_app.UtilTypes;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace mouse_tracking_web_app.ViewModels
{
    public class SettingsManager : INotifyPropertyChanged
    {
        #region settingsFiles

        // TODO: maybe save those somewhere else (like where I saved yolov5 and those stuff)

        private readonly string defaultSettingsPath = @"C:\ProgramData\MouseApp\Settings\defaultSettings.xml";

        private readonly string userSettingsPath = @"C:\ProgramData\MouseApp\Settings\userSettings.xml";

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
            SettingsInstance oldSettings = CurrentSettings.Copy();
            CurrentSettings.UpdateBasedOnSource(UpdatableSettings);
            NotifyUpdateChanges(oldSettings);
            NotifyPropertyChanged("CurrentSettings");
            UpdatableSettings = new SettingsInstance
            {
                OverrideDB = CurrentSettings.OverrideDB
            };
        }

        private void NotifyUpdateChanges(SettingsInstance oldSettings)
        {
            if (CurrentSettings.PythonPath != oldSettings.PythonPath)
                NotifyPropertyChanged("PythonPath");
            if (CurrentSettings.WorkingPath != oldSettings.WorkingPath)
                NotifyPropertyChanged("WorkingPath");
            if (CurrentSettings.DEPath != oldSettings.DEPath)
                NotifyPropertyChanged("DEPath");
            if (CurrentSettings.ConnectionString != oldSettings.ConnectionString)
                NotifyPropertyChanged("ConnectionString");
            if (CurrentSettings.DatabaseName != oldSettings.DatabaseName)
                NotifyPropertyChanged("DatabaseName");
            if (CurrentSettings.FileTypesList != oldSettings.FileTypesList)
                NotifyPropertyChanged("FileTypesList");
            if (CurrentSettings.VideoTypesList != oldSettings.VideoTypesList)
                NotifyPropertyChanged("VideoTypesList");
            if (CurrentSettings.PlotMarkerSize != oldSettings.PlotMarkerSize)
                NotifyPropertyChanged("PlotMarkerSize");
            NotifyPropertyChanged("OverrideDB");
        }

        #endregion settingsMethods

        private List<string> featuresList;

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
                NotifyPropertyChanged("WorkingPath");
                NotifyPropertyChanged("DEPath");
                NotifyPropertyChanged("ConnectionString");
                NotifyPropertyChanged("DatabaseName");
                NotifyPropertyChanged("FileTypesList");
                NotifyPropertyChanged("VideoTypesList");
                NotifyPropertyChanged("PlotMarkerSize");
                NotifyPropertyChanged("OverrideDB");
            }
        }

        public SettingsInstance UpdatableSettings
        {
            get => updatableSettings;
            set
            {
                updatableSettings = value;
                updatableSettings.OverrideDB = CurrentSettings.OverrideDB;
                NotifyPropertyChanged("UpdatableSettings");
            }
        }

        #endregion settingsInstances

        public string ConnectionString => CurrentSettings.ConnectionString;

        public string DatabaseName => CurrentSettings.DatabaseName;
        public string DEPath => CurrentSettings.DEPath;
        public List<string> FeaturesList => featuresList;

        public List<string> FileTypesList => new List<string>(CurrentSettings.FullTypesList.Split(','));

        public bool OverrideDB => CurrentSettings.OverrideDB;

        public double PlotMarkerSize => CurrentSettings.PlotMarkerSize;

        public string PythonPath => CurrentSettings.PythonPath;

        public List<string> VideoTypesList => new List<string>(CurrentSettings.VideoTypesList.Split(','));

        public string WorkingPath => CurrentSettings.WorkingPath;

        public string FirstCharToUpperCase(string str)
        {
            return !string.IsNullOrEmpty(str) && char.IsLower(str[0])
                ? str.Length == 1 ? char.ToUpper(str[0]).ToString() : char.ToUpper(str[0]) + str.Substring(1)
                : str;
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == "DEPath")
            {
                SetFeaturesList();
                NotifyPropertyChanged("FeaturesList");
            }
        }

        private void SetFeaturesList()
        {
            string[] lines = File.ReadAllLines($"{DEPath}\\project_config.yaml");
            featuresList = new List<string>();
            bool flag = false;

            foreach (string line in lines)
            {
                if (line.EndsWith("class_names:"))
                    flag = true;
                else if (flag)
                {
                    if (line.Contains("-") && !line.Contains(":"))
                    {
                        string[] fLine = line.Split(' ');
                        string feature = fLine[fLine.Length - 1];
                        if (feature != "background")
                            featuresList.Add(feature);
                    }
                    else return;
                }
            }
        }
    }
}