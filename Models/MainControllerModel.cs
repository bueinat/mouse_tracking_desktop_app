﻿using mouse_tracking_web_app.DataBase;
using mouse_tracking_web_app.UtilTypes;
using mouse_tracking_web_app.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace mouse_tracking_web_app.Models
{
    public class MainControllerModel : INotifyPropertyChanged
    {
        private Analysis analysis;
        private bool dragEnabled = false;
        private bool isLoading = false;
        private bool pause = true;
        private bool stop = false;
        private bool videoProcessed = false;

        public MainControllerModel(SettingsManager sManager)
        {
            DBHandler = new DataBaseHandler(this, sManager);
            DBHandler.Connect();
            CodeRunner = new OuterCodeRunner(sManager);
            VC = new VideoControllerModel(this);
            PC = new PlottingControllerModel(this, sManager);
            SM = sManager;
            SM.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged(e.PropertyName);
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DataRows AnalysisDataRows => VideoStats?.DataRows;
        public double AverageAcceleration => VideoStats is null ? 0 : VideoStats.AverageAcceleration;
        public double AverageSpeed => VideoStats is null ? 0 : VideoStats.AverageSpeed;
        public OuterCodeRunner CodeRunner { get; }
        public string CSVString => VideoAnalysis.GetCSVString(CachePath);
        public DataBaseHandler DBHandler { get; }

        public string DEPath => SM.DEPath;

        public bool DragEnabled
        {
            get => dragEnabled;
            set
            {
                dragEnabled = value;
                NotifyPropertyChanged("DragEnabled");
            }
        }

        public Dictionary<string, double> FeaturesPercents => VideoStats?.FeaturesPercents;

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                NotifyPropertyChanged("IsLoading");
            }
        }

        public int NSteps => VideoStats is null ? 0 : VideoStats.NSteps;
        public bool OverrideDB => SM.OverrideDB;

        public bool Pause
        {
            get => pause;
            set
            {
                pause = value;
                NotifyPropertyChanged("Pause");
            }
        }

        public PlottingControllerModel PC { get; }

        public SettingsManager SM { get; }

        public bool Stop
        {
            get => stop;
            set
            {
                stop = value;
                NotifyPropertyChanged("Stop");
            }
        }

        public double TotalDistance => VideoStats is null ? 0 : VideoStats.TotalDistance;
        public VideoControllerModel VC { get; }

        public Analysis VideoAnalysis
        {
            get => analysis;
            set
            {
                analysis = value;
                VideoStats = new AnalysisStats(VideoAnalysis);
                NotifyPropertyChanged("VideoAnalysis");
                NotifyPropertyChanged("VideoStats");
                NotifyPropertyChanged("AverageSpeed");
                NotifyPropertyChanged("AverageAcceleration");
                NotifyPropertyChanged("NSteps");
                NotifyPropertyChanged("FeaturesPercents");
                NotifyPropertyChanged("FeaturesDictionary");
                NotifyPropertyChanged("TotalDistance");
                NotifyPropertyChanged("AnalysisDataRows");
                NotifyPropertyChanged("CSVString");
            }
        }

        public bool VideoProcessed
        {
            get => videoProcessed;
            set
            {
                videoProcessed = value;
                NotifyPropertyChanged("VideoProcessed");
                NotifyPropertyChanged("IsVideoLoaded");
            }
        }

        public AnalysisStats VideoStats { get; set; }
        public List<string> VideoTypesList => SM.VideoTypesList;

        #region selectedVideo

        private DisplayableVideo selectedVideo;

        public DisplayableVideo SelectedVideo
        {
            get => selectedVideo;
            set
            {
                selectedVideo = value;
                NotifyPropertyChanged("SelectedVideo");
                NotifyPropertyChanged("VideoName");
            }
        }

        public string VideoName => SelectedVideo.ReducedName;

        #endregion selectedVideo

        #region videosPath

        private string videosPath;

        //public string CachePath => VideosPath is null
        //            ? ""
        //            : File.GetAttributes(VideosPath).HasFlag(FileAttributes.Directory)
        //            ? $"{VideosPath}\\.cache"
        //            : $"{Path.GetDirectoryName(VideosPath)}\\.cache";
        public string CachePath => DEPath;

        public List<string> VideosList { get; set; }

        public string VideosPath
        {
            get => videosPath;
            set
            {
                videosPath = value;
                VideosList = GetVideosList();
                NotifyPropertyChanged("VideosPath");
                NotifyPropertyChanged("VideosList");
                //_ = ProcessFolder();
            }
        }

        private List<string> GetVideosList()
        {
            if (File.GetAttributes(VideosPath).HasFlag(FileAttributes.Directory))
            {
                List<string> l = new List<string>();
                foreach (string file in Directory.EnumerateFiles(VideosPath, "*.*", SearchOption.AllDirectories))
                {
                    if (!file.Contains(CachePath) && VideoTypesList.Any(s => file.EndsWith(s)))
                        l.Add(file);
                }
                return l;
            }
            else
            {
                return new List<string>() { VideosPath };
            }
        }

        #endregion videosPath

        public string WorkingPath => SM.WorkingPath;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void StopMethod()
        {
            Stop = true;
        }
    }
}