using mouse_tracking_web_app.DataBase;
using System.Collections.Generic;

using System.ComponentModel;
using System.Configuration;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.Models
{
    public class MainControllerModel : INotifyPropertyChanged
    {
        private Analysis analysis;
        private bool dragEnabled = false;
        private string errorMessage = "";
        private string fileExplorerDirectory = "";
        private bool isLoading = false;
        private bool overrideInDB;
        private bool pause = true;
        private string videoID;
        private string videoName = "";
        private bool videoProcessed = false;

        public MainControllerModel()
        {
            DBHandler = new DataBaseHandler(this);
            DBHandler.Connect();
            CodeRunner = new OuterCodeRunner();
            VC = new VideoControllerModel(this);
            PC = new PlottingControllerModel(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DataRows AnalysisDataRows => VideoStats?.DataRows;

        public string ArchivePath => $"{FileExplorerDirectory}\\archive";

        public double AverageAcceleration => VideoStats is null ? 0 : VideoStats.AverageAcceleration;

        public double AverageSpeed => VideoStats is null ? 0 : VideoStats.AverageSpeed;

        public OuterCodeRunner CodeRunner { get; }

        public string CSVString => VideoAnalysis.GetCSVString(ArchivePath);

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

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                NotifyPropertyChanged("ErrorMessage");
                NotifyPropertyChanged("HasErrorMessage");
            }
        }

        public string FileExplorerDirectory
        {
            get => fileExplorerDirectory;
            set
            {
                fileExplorerDirectory = value;
                NotifyPropertyChanged("FileExplorerDirectory");
            }
        }

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

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

        public bool IsVideoLoaded => !string.IsNullOrEmpty(VideoName) && VideoProcessed;

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

        public string VideoName
        {
            get => videoName;
            set
            {
                videoName = value;
                NotifyPropertyChanged("VideoName");
                _ = ProcessVideo(value);
                NotifyPropertyChanged("IsVideoLoaded");
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

        public Dictionary<string, string> ProcessResult(string[] rawResult)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            for (int i = 0; i < rawResult.Length; i++)
            {
                if (rawResult[i].StartsWith("e"))
                    result["ErrorMessage"] = rawResult[i].Substring(7);
                if (rawResult[i].StartsWith("message"))
                    result["Message"] = rawResult[i].Substring(9);
                if (rawResult[i].StartsWith("video id"))
                    result["VideoID"] = rawResult[i].Substring(10);
            }
            if (result.ContainsKey("Message") && (!result.ContainsKey("ErrorMessage")))
                result["ErrorMessage"] = result["Message"];
            NotifyPropertyChanged("IsVideoLoaded");
            return result;
        }

        public async Task ProcessVideo(string videoPath)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            string script = @"OutsideCode\ProcessVideoScript.py";
            string connectionString = ConfigurationManager.AppSettings.Get("ConnectionString");
            string dbName = ConfigurationManager.AppSettings.Get("DataBaseName");
            Dictionary<string, string> argv = new Dictionary<string, string>
            {
                ["override"] = OverrideInDB ? "True" : "False",
                ["connection_string"] = $"{connectionString}/{dbName}",
                ["video_path"] = videoPath,
                ["archive_path"] = ArchivePath
            };
            string[] rawResult = await CodeRunner.RunCmd(script, argv);
            Dictionary<string, string> processedResult = ProcessResult(rawResult);
            videoID = null;
            // TODO: turn videoID to null + make it a feature
            if (processedResult.ContainsKey("ErrorMessage"))
                ErrorMessage = processedResult["ErrorMessage"];
            if (processedResult.ContainsKey("VideoID"))
                videoID = processedResult["VideoID"];
            //if (processedResult.ContainsKey("ArchivePath"))
            //    ArchivePath = processedResult["ArchivePath"];
            IsLoading = false;
            OverrideInDB = false;
            if (VC.InitializeVideo(videoID))
            {
                VideoProcessed = true;
                VC.Run();
            }
        }

        public void StopMethod()
        {
            Stop = true;
        }
    }
}