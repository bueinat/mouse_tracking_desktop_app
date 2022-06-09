﻿using mouse_tracking_web_app.DataBase;
using System.Collections.Generic;

using System.ComponentModel;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.Models
{
    public class MainControllerModel : INotifyPropertyChanged
    {
        private Analysis analysis;
        private string errorMessage = "";
        private string fileExplorerDirectory = "";

        private bool isLoading = false;
        private bool pause = true;
        private string videoID;
        private string videoName = "";

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
        public string ArchivePath { get; set; }
        public double AverageAcceleration => VideoStats is null ? 0 : VideoStats.AverageAcceleration;
        public double AverageSpeed => VideoStats is null ? 0 : VideoStats.AverageSpeed;
        public OuterCodeRunner CodeRunner { get; }
        public string CSVString => VideoAnalysis.GetCSVString();
        public DataBaseHandler DBHandler { get; }

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

        private bool dragEnabled = false;
        public bool DragEnabled
        {
            get => dragEnabled;
            set
            {
                dragEnabled = value;
                NotifyPropertyChanged("DragEnabled");
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
        public bool IsVideoLoaded => !string.IsNullOrEmpty(VideoName);
        public int NSteps => VideoStats is null ? 0 : VideoStats.NSteps;
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
                //if (rawResult[i].StartsWith("video path"))
                //    result["VideoPath"] = rawResult[i].Substring(12);
                if (rawResult[i].StartsWith("archive path"))
                    result["ArchivePath"] = rawResult[i].Substring(13);
                //if (rawResult[i].StartsWith("nframes"))
                //    result["FramesNumber"] = rawResult[i].Substring(9);
                if (rawResult[i].StartsWith("video id"))
                    result["VideoID"] = rawResult[i].Substring(10);
            }
            NotifyPropertyChanged("IsVideoLoaded");
            return result;
        }

        public async Task ProcessVideo(string videoPath)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            string script = @"OutsideCode\ProcessVideoScript.py";
            List<string> argv = new List<string>
            {
                videoPath
            };
            string[] rawResult = await CodeRunner.RunCmd(script, argv);
            Dictionary<string, string> processedResult = ProcessResult(rawResult);
            if (processedResult.ContainsKey("ErrorMessage"))
                ErrorMessage = processedResult["ErrorMessage"];
            if (processedResult.ContainsKey("VideoID"))
                videoID = processedResult["VideoID"];
            if (processedResult.ContainsKey("ArchivePath"))
                ArchivePath = processedResult["ArchivePath"];
            IsLoading = false;
            VC.InitializeVideo(videoID);
            VC.Run();
        }

        public void StopMethod()
        {
            Stop = true;
        }
    }
}