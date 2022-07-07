using mouse_tracking_web_app.DataBase;
using mouse_tracking_web_app.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace mouse_tracking_web_app.Models
{
    public class MainControllerModel : INotifyPropertyChanged
    {
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
            }
        }

        #endregion selectedVideo

        #region videosPath

        private string videosPath;

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

        public List<string> VideosList { get; set; }

        public string CachePath => VideosPath is null
                    ? ""
                    : File.GetAttributes(VideosPath).HasFlag(FileAttributes.Directory)
                    ? $"{VideosPath}\\.cache"
                    : $"{Path.GetDirectoryName(VideosPath)}\\.cache";

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

        private Analysis analysis;
        private bool dragEnabled = false;
        //private string fileExplorerDirectory = "";
        private bool isLoading = false;
        private bool overrideInDB;
        private bool pause = true;
        //private string videoID;
        //private string videoName = "";
        private bool videoProcessed = false;

        public MainControllerModel(SettingsManager sManager)
        {
            DBHandler = new DataBaseHandler(this, sManager);
            DBHandler.Connect();
            CodeRunner = new OuterCodeRunner(sManager);
            VC = new VideoControllerModel(this);
            PC = new PlottingControllerModel(this, sManager);
            SM = sManager;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DataRows AnalysisDataRows => VideoStats?.DataRows;

        public double AverageAcceleration => VideoStats is null ? 0 : VideoStats.AverageAcceleration;

        public double AverageSpeed => VideoStats is null ? 0 : VideoStats.AverageSpeed;

        public OuterCodeRunner CodeRunner { get; }

        public string CSVString => VideoAnalysis.GetCSVString(CachePath);

        public DataBaseHandler DBHandler { get; }

        public bool DragEnabled
        {
            get => dragEnabled;
            set
            {
                dragEnabled = value;
                NotifyPropertyChanged("DragEnabled");
            }
        }

        public string FileExplorerDirectory => SM.WorkingPath;
        //{
        //    get => SM.WorkingPath;
        //    set
        //    {
        //        SM.WorkingPath = value;
        //        NotifyPropertyChanged("FileExplorerDirectory");
        //    }
        //}

        public double IsDrinkingPercent => VideoStats is null ? 0 : VideoStats.IsDrinkingPercent;

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                NotifyPropertyChanged("IsLoading");
            }
        }

        public double IsNoseCastingPercent => VideoStats is null ? 0 : VideoStats.IsNoseCastingPercent;

        public double IsSniffingPercent => VideoStats is null ? 0 : VideoStats.IsSniffingPercent;

        public int NSteps => VideoStats is null ? 0 : VideoStats.NSteps;

        public bool OverrideInDB
        {
            get => overrideInDB;
            set
            {
                overrideInDB = value;
                NotifyPropertyChanged("OverrideInDB");
            }
        }

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

        public bool Stop { get; set; }

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
                NotifyPropertyChanged("IsDrinkingPercent");
                NotifyPropertyChanged("IsNoseCastingPercent");
                NotifyPropertyChanged("IsSniffingPercent");
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