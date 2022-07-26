using mouse_tracking_web_app.UtilTypes;
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

        public void ProcessSingleVideo(object videoPath, object cancellationToken)
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
                ["connection_string"] = $"{connectionString}/{dbName}"
            };

            // run algorithm
            model.CodeRunner.RunCmd(@"OutsideCode\FullCode.py", argv, currentVideo.OutputHandler, currentVideo.ErrorHandler,
                                                                    (CancellationToken)cancellationToken);
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