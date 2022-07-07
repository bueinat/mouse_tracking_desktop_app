using mouse_tracking_web_app.DataBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace mouse_tracking_web_app.ViewModels
{
    public class VideoProcessingManager : INotifyPropertyChanged
    {
        #region notification_and_construction

        private readonly Models.MainControllerModel model;

        public VideoProcessingManager(Models.MainControllerModel mainController)
        {
            model = mainController;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("VPM_" + e.PropertyName);
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == "VPM_VideosPath")
                _ = ProcessFolder();
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

        private BindingList<DisplayableVideo> videosCollection = new BindingList<DisplayableVideo>();
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

        public async Task ProcessFolder()
        {
            videosDictionary = new Dictionary<string, DisplayableVideo>();

            List<Task> tasks = new List<Task>();
            VideosCollection = new BindingList<DisplayableVideo>();

            foreach (string videoPath in VPM_VideosList)
            {
                string relativePath = MakeRelative(videoPath, VPM_VideosPath);
                relativePath = relativePath.Substring(relativePath.IndexOf('\\') + 1);
                videosDictionary.Add(videoPath, new DisplayableVideo()
                {
                    ReducedName = relativePath,
                    ProcessingState = DisplayableVideo.State.Waiting
                });
                VideosCollection.Add(videosDictionary[videoPath]);

                //await ProcessSingleVideo(videoPath);

                tasks.Add(ProcessSingleVideo(videoPath));
            }
            //_ = Parallel.ForEach(VPM_VideosList, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, async videoPath => await ProcessSingleVideo(videoPath));
            //await LimitProcessedTasks(tasks, );
            await Task.WhenAll(tasks);
        }

        //private async Task LimitProcessedTasks<T>(IEnumerable<T> items, Func<T, Task> func)
        //{
        //    ExecutionDataflowBlockOptions edfbo = new ExecutionDataflowBlockOptions
        //    {
        //        MaxDegreeOfParallelism = 2
        //    };

        //    ActionBlock<T> ab = new ActionBlock<T>(func, edfbo);

        //    foreach (T item in items)
        //    {
        //        _ = await ab.SendAsync(item);
        //    }

        //    ab.Complete();
        //    await ab.Completion;
        //}

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
            return result;
        }

        public async Task ProcessSingleVideo(string videoPath)
        {
            // initialization
            DisplayableVideo currentVideo = videosDictionary[videoPath];

            string connectionString = model.SM.ConnectionString;
            string dbName = model.SM.DatabaseName;

            Dictionary<string, string> argv = new Dictionary<string, string>
            {
                ["override"] = model.OverrideDB ? "True" : "False",
                ["video_path"] = videoPath,
                ["data_path"] = $"{VPM_CachePath}\\{currentVideo.ReducedName.Split('.')[0]}",
                ["connection_string"] = $"{connectionString}/{dbName}"
            };

            // run first
            currentVideo.ProcessingState = DisplayableVideo.State.ExtractVideo;
            Dictionary<string, string> processedResult = await RunPhaseUpdateError(@"OutsideCode\ExtractVideo.py", argv, videoPath);
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
            _ = await RunPhaseUpdateError(@"OutsideCode\FindRatPath.py", argv, videoPath);
            if (currentVideo.HasFailed())
                return;

            // run third section
            currentVideo.ProcessingState = DisplayableVideo.State.FindRatFeatues;
            _ = await RunPhaseUpdateError(@"OutsideCode\FindRatFeatures.py", argv, videoPath);
            if (currentVideo.HasFailed())
                return;

            // run fourth section
            currentVideo.ProcessingState = DisplayableVideo.State.SaveToDataBase;
            processedResult = await RunPhaseUpdateError(@"OutsideCode\SaveToDataBase.py", argv, videoPath);
            if (currentVideo.HasFailed())
                return;
            currentVideo.ProcessingState = DisplayableVideo.State.Successful;
            currentVideo.VideoID = processedResult["VideoID"];
            //currentVideo.VideoItem = model.DBHandler.GetVideoByID(currentVideo.VideoID);
        }

        public async Task<Dictionary<string, string>> RunPhaseUpdateError(string scriptPath, Dictionary<string, string> argv, string videoPath)
        {
            // run and process
            string[] rawResult = await model.CodeRunner.RunCmd(scriptPath, argv);
            Dictionary<string, string> processedResult = ProcessResult(rawResult);

            // update tooltip message
            string key = null;
            if (processedResult.ContainsKey("ErrorMessage"))
                key = "ErrorMessage";
            else if (processedResult.ContainsKey("Message"))
                key = "Message";
            if (!string.IsNullOrEmpty(key))
            {
                if (videosDictionary[videoPath].ProcessingState == DisplayableVideo.State.ExtractVideo)
                    videosDictionary[videoPath].ToolTipMessage += "\r\n";
                videosDictionary[videoPath].ToolTipMessage += $"{videosDictionary[videoPath].ProcessingState}: {processedResult[key]}";
            }
            if (processedResult["Success"] == "False")
            {
                if (!processedResult.ContainsKey("ErrorMessage"))
                    videosDictionary[videoPath].ToolTipMessage += $"\r\nUnknown Error at {videosDictionary[videoPath].ProcessingState}";
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