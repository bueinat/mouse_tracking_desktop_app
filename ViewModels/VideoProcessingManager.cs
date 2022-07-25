using mouse_tracking_web_app.DataBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace mouse_tracking_web_app.ViewModels
{
    // Sunday:
    // * apply stop button
    // * add advanced settings
    // * order the list
    // * user controls take time
    // * features list fix
    // * create a working project, zip and email it
    // * start writing an installation guide

    public class VideoProcessingManager : INotifyPropertyChanged
    {
        #region notification_and_construction

        private readonly Models.MainControllerModel model;
        private object _lock;

        public VideoProcessingManager(Models.MainControllerModel mainController)
        {
            model = mainController;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("VPM_" + e.PropertyName);
            };
            videosCollection = new BindingList<DisplayableVideo>();
            _lock = new object();
            BindingOperations.EnableCollectionSynchronization(videosCollection, _lock);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == "VPM_VideosPath")
            {
                VideosCollection.Clear();
                _ = Task.Run(ProcessFolder);
            }
        }

        #endregion notification_and_construction

        #region selectedVideo

        public DisplayableVideo VPM_SelectedVideo
        {
            get => model.SelectedVideo;
            set
            {
                model.SelectedVideo = value;
                NotifyPropertyChanged("VPM_SelectedVideo");
                if (!(VPM_SelectedVideo is null) && model.VC.InitializeVideo(VPM_SelectedVideo.VideoID))
                    model.VC.Run();
            }
        }

        #endregion selectedVideo

        #region videosPath

        public string VPM_CachePath => model.CachePath;

        public List<string> VPM_VideosList => model.VideosList;

        public string VPM_VideosPath
        {
            get => model.VideosPath;
            set => model.VideosPath = value;
        }

        #endregion videosPath

        #region videosContainer

        private BindingList<DisplayableVideo> videosCollection;
        private Dictionary<string, DisplayableVideo> videosDictionary;

        public BindingList<DisplayableVideo> VideosCollection
        {
            get => videosCollection;
            set
            {
                videosCollection = value;
                NotifyPropertyChanged("VideosCollection");
            }
        }

        #endregion videosContainer

        #region processingMethods

        public void ProcessFolder()
        {
            videosDictionary = new Dictionary<string, DisplayableVideo>();

            List<Task> tasks = new List<Task>();
            int maxConcurrency = 2;

            using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(maxConcurrency))
            {
                foreach (string videoPath in VPM_VideosList)
                {
                    // create video and add it to the list
                    string relativePath = MakeRelative(videoPath, VPM_VideosPath);
                    relativePath = relativePath.Substring(relativePath.IndexOf('\\') + 1);
                    if (string.IsNullOrEmpty(relativePath))
                        relativePath = videoPath.Split('\\')[videoPath.Split('\\').Length - 1];
                    videosDictionary.Add(videoPath, new DisplayableVideo()
                    {
                        ReducedName = relativePath,
                        ProcessingState = DisplayableVideo.State.Waiting
                    });
                    lock (_lock)
                    {
                        VideosCollection.Add(videosDictionary[videoPath]);
                    }
                    tasks.Add(videosDictionary[videoPath].Start(ProcessSingleVideo, videoPath, concurrencySemaphore));
                }
                Task.WaitAll(tasks.ToArray());
            }
        }

        public Dictionary<string, string> ProcessResult(List<string> rawResult)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (string line in rawResult)
            {
                if (line.StartsWith("e"))
                    result["ErrorMessage"] = line.Substring(7);
                if (line.StartsWith("message"))
                    result["Message"] = line.Substring(9);
                if (line.StartsWith("video id"))
                    result["VideoID"] = line.Substring(10);
                if (line.StartsWith("override"))
                    result["Override"] = line.Substring(10);
                if (line.StartsWith("nframes"))
                    result["NFrames"] = line.Substring(9);
                if (line.StartsWith("success"))
                    result["Success"] = "True";
            }
            if (!result.ContainsKey("Success"))
                result["Success"] = "False";
            return result;
        }

        public void ProcessSingleVideo(object videoPath)
        {
            // initialization
            string vidPath = (string)videoPath;
            DisplayableVideo currentVideo = videosDictionary[vidPath];

            string connectionString = model.SM.ConnectionString;
            string dbName = model.SM.DatabaseName;

            Dictionary<string, string> argv = new Dictionary<string, string>
            {
                ["override"] = model.OverrideDB ? "True" : "False",
                ["video_path"] = vidPath,
                ["data_path"] = $"{VPM_CachePath}\\{currentVideo.ReducedName.Split('.')[0]}",
                ["de_project_path"] = @"C:\Users\Public\MouseTracking\NewProject_deepethogram",
                ["connection_string"] = $"{connectionString}/{dbName}"
            };
            // TODO: add project path to settings

            // run first
            currentVideo.ProcessingState = DisplayableVideo.State.ExtractVideo;
            Dictionary<string, string> processedResult = RunPhaseUpdateError(@"OutsideCode\ExtractVideo.py", argv, vidPath);
            if (currentVideo.HasFailed())
                return;

            // if finished, update videoID and return
            if (processedResult.ContainsKey("VideoID"))
            {
                currentVideo.VideoID = processedResult["VideoID"];
                currentVideo.ProcessingState = DisplayableVideo.State.Successful;
                return;
            }
            // update arguments
            argv["nframes"] = processedResult["NFrames"];
            argv["override"] = processedResult["Override"];

            // run second section
            currentVideo.ProcessingState = DisplayableVideo.State.FindRatPath;
            RunPhaseUpdateError(@"OutsideCode\FindRatPath.py", argv, vidPath);
            if (currentVideo.HasFailed())
                return;

            // run third section
            currentVideo.ProcessingState = DisplayableVideo.State.FindRatFeatues;
            RunPhaseUpdateError(@"OutsideCode\FindRatFeatures.py", argv, vidPath);
            if (currentVideo.HasFailed())
                return;

            // run fourth section
            currentVideo.ProcessingState = DisplayableVideo.State.SaveToDataBase;
            processedResult = RunPhaseUpdateError(@"OutsideCode\SaveToDataBase.py", argv, vidPath);
            if (currentVideo.HasFailed())
                return;
            currentVideo.ProcessingState = DisplayableVideo.State.Successful;
            currentVideo.VideoID = processedResult["VideoID"];
            return;
            //currentVideo.VideoItem = model.DBHandler.GetVideoByID(currentVideo.VideoID);
        }

        public Dictionary<string, string> RunPhaseUpdateError(string scriptPath, Dictionary<string, string> argv, string videoPath)
        {
            // run and process
            List<string> rawResult = model.CodeRunner.RunCmd(scriptPath, argv);
            Dictionary<string, string> processedResult = ProcessResult(rawResult);

            // update tooltip message
            string key = null;
            if (processedResult.ContainsKey("ErrorMessage"))
                key = "ErrorMessage";
            else if (processedResult.ContainsKey("Message"))
                key = "Message";
            if (!string.IsNullOrEmpty(key))
            {
                if (videosDictionary[videoPath].ProcessingState != DisplayableVideo.State.ExtractVideo)
                    videosDictionary[videoPath].ToolTipMessage += "\r\n";
                videosDictionary[videoPath].ToolTipMessage += $"{videosDictionary[videoPath].ProcessingState}: {processedResult[key]}";
            }
            if (processedResult["Success"] == "False")
            {
                if (!processedResult.ContainsKey("ErrorMessage"))
                {
                    if (videosDictionary[videoPath].ProcessingState != DisplayableVideo.State.ExtractVideo)
                        videosDictionary[videoPath].ToolTipMessage += "\r\n";
                    videosDictionary[videoPath].ToolTipMessage += $"\r\nUnknown Error at {videosDictionary[videoPath].ProcessingState}";
                }
                videosDictionary[videoPath].ProcessingState = DisplayableVideo.State.Failed;
            }

            // return processed results
            return processedResult;
        }

        #endregion processingMethods

        private static string MakeRelative(string filePath, string referencePath)
        {
            Uri fileUri = new Uri(filePath);
            Uri referenceUri = new Uri(referencePath);
            return Uri.UnescapeDataString(referenceUri.MakeRelativeUri(fileUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
        }
    }
}