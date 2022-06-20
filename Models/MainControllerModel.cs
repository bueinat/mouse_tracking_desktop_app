﻿using mouse_tracking_web_app.DataBase;
using mouse_tracking_web_app.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.Models
{
    public class MainControllerModel : INotifyPropertyChanged
    {
        private readonly List<string> videoTypesList = new List<string>(ConfigurationManager.AppSettings["VideoTypesList"].Split(','));

        #region selectedVideo

        private DisplayableVideo selectedVideo;
        public DisplayableVideo SelectedVideo
        {
            get => selectedVideo;
            set
            {
                selectedVideo = value;
                NotifyPropertyChanged("SelectedVideo");
                if (VC.InitializeVideo(SelectedVideo.VideoID))
                {
                    VideoProcessed = true;
                    VC.Run();
                }
            }
        }

        #endregion selectedVideo

        private Analysis analysis;
        private Dictionary<string, DisplayableVideo> displayableVideosDict;
        private BindingList<DisplayableVideo> dispVideosCollection = new BindingList<DisplayableVideo>();
        private bool dragEnabled = false;
        private string errorMessage = "";
        private string fileExplorerDirectory = "";
        private bool isLoading = false;
        private bool overrideInDB;
        private bool pause = true;
        private string videoID;
        private string videoName = "";
        private bool videoProcessed = false;

        private string videosPath;

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

        public double AverageAcceleration => VideoStats is null ? 0 : VideoStats.AverageAcceleration;

        public double AverageSpeed => VideoStats is null ? 0 : VideoStats.AverageSpeed;

        public string CachePath
        {
            get
            {
                if (VideosPath is null)
                    return "";
                return File.GetAttributes(VideosPath).HasFlag(FileAttributes.Directory)
                    ? $"{VideosPath}\\.cache"
                    : $"{Path.GetDirectoryName(VideosPath)}\\.cache";
            }
        }
        public OuterCodeRunner CodeRunner { get; }

        public string CSVString => VideoAnalysis.GetCSVString(CachePath);

        public DataBaseHandler DBHandler { get; }

        public BindingList<DisplayableVideo> DispVideosCollection
        {
            get => dispVideosCollection;
            set
            {
                dispVideosCollection = value;
                NotifyPropertyChanged("DispVideosCollection");
            }
        }

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
                _ = ProcessVideo();
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
                _ = ProcessFolder();
            }
        }

        public AnalysisStats VideoStats { get; set; }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task ProcessFolder()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            displayableVideosDict = new Dictionary<string, DisplayableVideo>();

            //List<Task> tasks = new List<Task>();
            DispVideosCollection = new BindingList<DisplayableVideo>();

            foreach (string videoPath in VideosList)
            {
                //string relativeVideoPath = MakeRelative(videoPath, VideosPath);
                displayableVideosDict.Add(videoPath, new DisplayableVideo());
                DispVideosCollection.Add(displayableVideosDict[videoPath]);
                await ProcessSingleVideo(videoPath);

                //tasks.Add(ProcessSingleVideo(videoPath));
            }
            //await Task.WhenAll(tasks);

            IsLoading = false;
            //OverrideInDB = false;
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
                if (rawResult[i].StartsWith("override"))
                    result["Override"] = rawResult[i].Substring(10);
                if (rawResult[i].StartsWith("nframes"))
                    result["NFrames"] = rawResult[i].Substring(9);
                if (rawResult[i].StartsWith("success"))
                    result["Success"] = "True";
            }
            if (!result.ContainsKey("Success"))
                result["Success"] = "False";
            if (result.ContainsKey("Message") && (!result.ContainsKey("ErrorMessage")))
                result["ErrorMessage"] = result["Message"];
            NotifyPropertyChanged("IsVideoLoaded");
            return result;
        }

        public async Task ProcessSingleVideo(string videoPath)
        {
            string relativeVideoPath = MakeRelative(videoPath, VideosPath).Split('.')[0];
            string errorKey = "ErrorMessage";
            displayableVideosDict[videoPath].ProcessingState = DisplayableVideo.State.ExtractVideo;
            displayableVideosDict[videoPath].ReducedName = MakeRelative(videoPath, VideosPath);

            string connectionString = ConfigurationManager.AppSettings.Get("ConnectionString");
            string dbName = ConfigurationManager.AppSettings.Get("DataBaseName");

            Dictionary<string, string> argv = new Dictionary<string, string>
            {
                ["override"] = OverrideInDB ? "True" : "False",
                ["video_path"] = videoPath,
                ["data_path"] = $"{CachePath}\\{relativeVideoPath}",
                ["connection_string"] = $"{connectionString}/{dbName}"
            };

            // run first section
            Dictionary<string, string> processedResult = await RunPhaseUpdateError(@"OutsideCode\ExtractVideo.py", argv, videoPath);
            if (processedResult.ContainsKey(errorKey))
                displayableVideosDict[videoPath].ToolTipMessage += $"ExtractVideo: {processedResult[errorKey]}";

            if (processedResult.ContainsKey("VideoID"))
            {
                displayableVideosDict[videoPath].VideoID = processedResult["VideoID"];
                displayableVideosDict[videoPath].VideoItem = DBHandler.GetVideoByID(displayableVideosDict[videoPath].VideoID);
                if (DBHandler.DoesIDexist(displayableVideosDict[videoPath].VideoItem.Analysis, "analysis"))
                    displayableVideosDict[videoPath].ProcessingState = DisplayableVideo.State.Successful;
                else
                {
                    displayableVideosDict[videoPath].ProcessingState = DisplayableVideo.State.Failed;
                    displayableVideosDict[videoPath].ToolTipMessage += "\r\nUnknown Error at ExtractVideo";
                }
            }
            //_videoID = processedResult["VideoID"];

            if (processedResult["Success"] == "False")
            {
                displayableVideosDict[videoPath].ProcessingState = DisplayableVideo.State.Failed;
            }
            if (!string.IsNullOrEmpty(displayableVideosDict[videoPath].VideoID) || displayableVideosDict[videoPath].ProcessingState == DisplayableVideo.State.Failed)
            {
                // create video element
                // return it
                return;
            }
            argv["nframes"] = processedResult["NFrames"];
            argv["override"] = processedResult["Override"];

            // run second section
            displayableVideosDict[videoPath].ProcessingState = DisplayableVideo.State.FindRatPath;
            processedResult = await RunPhaseUpdateError(@"OutsideCode\FindRatPath.py", argv, videoPath);
            if (processedResult.ContainsKey(errorKey))
                displayableVideosDict[videoPath].ToolTipMessage += $"\r\nFindRatPath: {processedResult[errorKey]}";
            if (processedResult["Success"] == "False")
            {
                displayableVideosDict[videoPath].ProcessingState = DisplayableVideo.State.Failed;
                displayableVideosDict[videoPath].ToolTipMessage += "\r\nUnknown Error at FindRatPath";
                return;
            }

            // run third section
            displayableVideosDict[videoPath].ProcessingState = DisplayableVideo.State.FindRatFeatues;
            processedResult = await RunPhaseUpdateError(@"OutsideCode\FindRatFeatures.py", argv, videoPath);
            if (processedResult.ContainsKey(errorKey))
                displayableVideosDict[videoPath].ToolTipMessage += $"\r\nFindRatFeatues: {processedResult[errorKey]}";
            if (processedResult["Success"] == "False")
            {
                displayableVideosDict[videoPath].ProcessingState = DisplayableVideo.State.Failed;
                displayableVideosDict[videoPath].ToolTipMessage += "\r\nUnknown Error at FindRatFeatures";
                return;
            }

            // run fourth section
            displayableVideosDict[videoPath].ProcessingState = DisplayableVideo.State.SaveToDataBase;
            processedResult = await RunPhaseUpdateError(@"OutsideCode\SaveToDataBase.py", argv, videoPath);
            if (processedResult.ContainsKey(errorKey))
                displayableVideosDict[videoPath].ToolTipMessage += $"\r\nSaveToDataBase: {processedResult[errorKey]}";
            displayableVideosDict[videoPath].VideoID = processedResult["VideoID"];
            displayableVideosDict[videoPath].VideoItem = DBHandler.GetVideoByID(displayableVideosDict[videoPath].VideoID);
            if (processedResult["Success"] == "True")
                displayableVideosDict[videoPath].ProcessingState = DisplayableVideo.State.Successful;
            else
            {
                displayableVideosDict[videoPath].ProcessingState = DisplayableVideo.State.Failed;
                displayableVideosDict[videoPath].ToolTipMessage += "\r\nUnknown Error at SaveToDataBase";
            }
        }

        public async Task ProcessVideo()
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
                ["video_path"] = VideoName,
                ["cache_path"] = CachePath
            };
            string[] rawResult = await CodeRunner.RunCmd(script, argv);
            Dictionary<string, string> processedResult = ProcessResult(rawResult);
            videoID = null;
            // TODO: turn videoID to null + make it a feature
            if (processedResult.ContainsKey("ErrorMessage"))
                ErrorMessage = processedResult["ErrorMessage"];
            if (processedResult.ContainsKey("VideoID"))
                videoID = processedResult["VideoID"];
            IsLoading = false;
            OverrideInDB = false;
            if (VC.InitializeVideo(videoID))
            {
                VideoProcessed = true;
                VC.Run();
            }
        }

        public async Task<Dictionary<string, string>> RunPhaseUpdateError(string scriptPath, Dictionary<string, string> argv, string videoPath)
        {
            string[] rawResult = await CodeRunner.RunCmd(scriptPath, argv);
            Dictionary<string, string> processedResult = ProcessResult(rawResult);
            if (processedResult.ContainsKey("ErrorMessage"))
            {
                string eMessage = processedResult["ErrorMessage"];
                ErrorMessage = $"{ErrorMessage}\r\n{MakeRelative(videoPath, VideosPath).Split('.')[0]}: {eMessage}";
            }
            return processedResult;
        }

        public void StopMethod()
        {
            Stop = true;
        }

        private static string MakeRelative(string filePath, string referencePath)
        {
            Uri fileUri = new Uri(filePath);
            Uri referenceUri = new Uri(referencePath);
            return Uri.UnescapeDataString(referenceUri.MakeRelativeUri(fileUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
        }

        private List<string> GetVideosList()
        {
            if (File.GetAttributes(VideosPath).HasFlag(FileAttributes.Directory))
            {
                List<string> l = new List<string>();
                foreach (string file in Directory.EnumerateFiles(VideosPath, "*.*", SearchOption.AllDirectories))
                {
                    if (!file.Contains(CachePath) && videoTypesList.Any(s => file.EndsWith(s)))
                        l.Add(file);
                    //l.Add(MakeRelative(file, VideosPath));
                }
                return l;
            }
            else
            {
                return new List<string>() { VideosPath };
            }
        }
    }
}