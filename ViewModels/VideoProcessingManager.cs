using mouse_tracking_web_app.DataBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace mouse_tracking_web_app.ViewModels
{
    // Sunday:
    // * meeting with Osnat
    // * test the fix of features and point
    // * apply stop button
    // * add advanced settings
    // * order the list
    // * user controls take time
    // * features list fix
    
    
    
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
                Task.Run(ProcessFolder);
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

        public void ProcessFolder()
        {
            videosDictionary = new Dictionary<string, DisplayableVideo>();

            //List<Task> tasks = new List<Task>();
            VideosCollection = new BindingList<DisplayableVideo>();

            //LimitedConcurrencyLevelTaskScheduler lcts = new LimitedConcurrencyLevelTaskScheduler(2); // TODO: enable changing
            //List<Task> tasks = new List<Task>();

            // Create a TaskFactory and pass it our custom scheduler.
            //TaskFactory factory = new TaskFactory(lcts);
            //CancellationTokenSource cts = new CancellationTokenSource();

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
                VideosCollection.Add(videosDictionary[videoPath]);

                // run video


                // Use our factory to run a set of tasks.
                //Task t = factory.StartNew(ProcessSingleVideo, videoPath, cts.Token);
                //tasks.Add(t);

                //
                //videosDictionary[videoPath].Start(ProcessSingleVideo, videoPath);
                //

                //await ProcessSingleVideo(videoPath);

                //tasks.Add(ProcessSingleVideo(videoPath));
            }

            // TODO: change degree of parallelism
            Parallel.ForEach(VPM_VideosList, new ParallelOptions { MaxDegreeOfParallelism = 2 },
            videoPath =>
            {
                // logic
                ProcessSingleVideo(videoPath);
            });

            //// run all tasks
            //while (tasks.Count > 0)
            //{
            //    Task firstFinishedTask = await Task.WhenAny(tasks);
            //    _ = tasks.Remove(firstFinishedTask);
            //}


            //Task.WaitAll(tasks.ToArray());
            //cts.Dispose();
            //_ = Parallel.ForEach(VPM_VideosList, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, async videoPath => await ProcessSingleVideo(videoPath));
            //await LimitProcessedTasks(tasks, );
            //await Task.WhenAll(tasks);
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

        public void ProcessSingleVideo(string videoPath)
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

    // code taken from here: https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskscheduler?view=net-6.0
    // Provides a task scheduler that ensures a maximum concurrency level while
    // running on top of the thread pool.
    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        // Indicates whether the current thread is processing work items.
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;

        // The list of tasks to be executed
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks)

        // The maximum concurrency level allowed by this scheduler.
        private readonly int _maxDegreeOfParallelism;

        // Indicates whether the scheduler is currently processing work items.
        private int _delegatesQueuedOrRunning = 0;

        // Creates a new instance with the specified degree of parallelism.
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        // Queues a task to the scheduler.
        protected sealed override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough
            // delegates currently queued or running to process tasks, schedule another.
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        // Inform the ThreadPool that there's work to be executed for this scheduler.
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                // Note that the current thread is now processing work items.
                // This is necessary to enable inlining of tasks into this thread.
                _currentThreadIsProcessingItems = true;
                try
                {
                    // Process all available items in the queue.
                    while (true)
                    {
                        Task item;
                        lock (_tasks)
                        {
                            // When there are no more items to be processed,
                            // note that we're done processing, and get out.
                            if (_tasks.Count == 0)
                            {
                                --_delegatesQueuedOrRunning;
                                break;
                            }

                            // Get the next item from the queue
                            item = _tasks.First.Value;
                            _tasks.RemoveFirst();
                        }

                        // Execute the task we pulled out of the queue
                        base.TryExecuteTask(item);
                    }
                }
                // We're done processing items on the current thread
                finally { _currentThreadIsProcessingItems = false; }
            }, null);
        }

        // Attempts to execute the specified task on the current thread.
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // If this thread isn't already processing a task, we don't support inlining
            if (!_currentThreadIsProcessingItems) return false;

            // If the task was previously queued, remove it from the queue
            if (taskWasPreviouslyQueued)
                // Try to run the task.
                if (TryDequeue(task))
                    return base.TryExecuteTask(task);
                else
                    return false;
            else
                return base.TryExecuteTask(task);
        }

        // Attempt to remove a previously scheduled task from the scheduler.
        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks) return _tasks.Remove(task);
        }

        // Gets the maximum concurrency level supported by this scheduler.
        public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

        // Gets an enumerable of the tasks currently scheduled on this scheduler.
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken) return _tasks;
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }
    }
}