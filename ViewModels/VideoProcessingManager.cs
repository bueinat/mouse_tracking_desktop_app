using mouse_tracking_web_app.UtilTypes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace mouse_tracking_web_app.ViewModels
{
    public class VideoProcessingManager : INotifyPropertyChanged
    {
        #region notification_and_construction

        private readonly object _lock;
        private readonly int _maxConcurrency;
        private readonly Models.MainControllerModel _model;

        /// <summary>
        /// This Constructor creates a new <c>VideoProcessingManager</c> which follows
        /// changes in a <see cref="Models.MainControllerModel"/>.
        /// </summary>
        /// <param name="mainController">a <see cref="Models.MainControllerModel"/> to follow changes in.</param>
        public VideoProcessingManager(Models.MainControllerModel mainController)
        {
            _model = mainController;

            _model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("VPM_" + e.PropertyName);
            };

            _lock = new object();
            _maxConcurrency = 2;

            _videosCollection = new BindingList<DisplayableVideo>();
            BindingOperations.EnableCollectionSynchronization(_videosCollection, _lock);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // if videos path is changed, process the whole folder.
            if (propertyName == "VPM_VideosPath")
            {
                VideosCollection.Clear();
                Task.Run(ProcessFolder);
            }
        }

        #endregion notification_and_construction

        #region selectedVideo

        /// <summary>
        /// Property <c>VPM_SelectedVideo</c> represents the video which should be displayed in View.
        /// </summary>
        public DisplayableVideo VPM_SelectedVideo
        {
            get => _model.SelectedVideo;
            set
            {
                _model.SelectedVideo = value;
                NotifyPropertyChanged("VPM_SelectedVideo");
                if (!(VPM_SelectedVideo is null) && (_model.VC.InitializeVideo(VPM_SelectedVideo.VideoID)))
                    _model.VC.Run();
            }
        }

        #endregion selectedVideo

        #region videosPath

        /// <summary>
        /// Property <c>VPM_CachePath</c> is the path of the cache folder where the data should be saved.
        /// </summary>
        public string VPM_CachePath => _model.CachePath;

        /// <summary>
        /// Property <c>VPM_VideosList</c> is a list of all videos in the current experiment.
        /// </summary>
        public List<string> VPM_VideosList => _model.VideosList;

        /// <summary>
        /// Property <c>VPM_VideosPath</c> is the path of the main directory of the experiment.
        /// </summary>
        public string VPM_VideosPath
        {
            get => _model.VideosPath;
            set => _model.VideosPath = value;
        }

        #endregion videosPath

        #region videosContainer

        private BindingList<DisplayableVideo> _videosCollection;
        private Dictionary<string, DisplayableVideo> _videosDictionary;

        /// <summary>
        /// Property <c>VideosCollection</c> is a list of <see cref="DisplayableVideo"/> items.
        /// They are being processed and displayed in the View.
        /// </summary>
        public BindingList<DisplayableVideo> VideosCollection
        {
            get => _videosCollection;
            set
            {
                _videosCollection = value;
                NotifyPropertyChanged("VideosCollection");
            }
        }

        #endregion videosContainer

        #region processingMethods

        /// <summary>
        /// Method <c>ProcessFolder</c> processes all the video in <see cref="VPM_VideosPath"/>
        /// </summary>
        public void ProcessFolder()
        {
            _videosDictionary = new Dictionary<string, DisplayableVideo>();
            List<Task> tasks = new List<Task>();

            using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(_maxConcurrency))
            {
                foreach (string videoPath in VPM_VideosList)
                {
                    // create video and add it to the list
                    string relativePath = Utils.UtilMethods.MakeRelative(videoPath, VPM_VideosPath);
                    if (string.IsNullOrEmpty(relativePath))
                        relativePath = videoPath.Split('\\')[videoPath.Split('\\').Length - 1];
                    _videosDictionary.Add(videoPath, new DisplayableVideo()
                    {
                        ReducedName = relativePath,
                        ProcessingState = DisplayableVideo.State.Waiting
                    });
                    // add the item to the videosCollection (could be done in parallel)
                    lock (_lock)
                    {
                        VideosCollection.Add(_videosDictionary[videoPath]);
                    }

                    // add processing task to list
                    tasks.Add(_videosDictionary[videoPath].Start(ProcessSingleVideo, videoPath, concurrencySemaphore));
                }

                // wait for all tasks to finish
                Task.WaitAll(tasks.ToArray());
            }
        }

        /// <summary>
        /// Method <c>ProcessSingleVideo</c> gets a video path and processes the given video.
        /// </summary>
        /// <param name="videoPath">Path of the video</param>
        /// <param name="cancellationToken">Token which allows cancellation of the processing.</param>
        public void ProcessSingleVideo(object videoPath, object cancellationToken)
        {
            // initialization
            string vidPath = (string)videoPath;
            DisplayableVideo currentVideo = _videosDictionary[vidPath];

            string connectionString = _model.SM.ConnectionString;
            string dbName = _model.SM.DatabaseName;

            // create a dictionary of arguments
            Dictionary<string, string> argv = new Dictionary<string, string>
            {
                ["override"] = _model.OverrideDB ? "True" : "False",
                ["video_path"] = vidPath,
                ["data_path"] = $"{VPM_CachePath}\\{currentVideo.ReducedName.Split('.')[0]}",
                ["connection_string"] = $"{connectionString}/{dbName}",
                ["de_project_path"] = @"C:\Users\Public\MouseTracking\NewProject_deepethogram"
            };

            // run algorithm
            _model.CodeRunner.RunCmd(@"OutsideCode\FullCode.py", argv, currentVideo.OutputHandler, currentVideo.ErrorHandler,
                                                                    (CancellationToken)cancellationToken);
        }

        #endregion processingMethods
    }
}