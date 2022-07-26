using mouse_tracking_web_app.DataBase;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.UtilTypes
{
    public class DisplayableVideo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void ErrorHandler(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine($"e: {e.Data}");
            }
        }

        public void OutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                // change state if needed (running states)
                if (e.Data == "FindRatPath")
                    ProcessingState = State.FindRatPath;
                else if (e.Data == "FindRatFeatures")
                    ProcessingState = State.FindRatFeatues;
                else if (e.Data == "SaveToDataBase")
                    ProcessingState = State.SaveToDataBase;

                // get video ID if given
                else if (e.Data.StartsWith("video id"))
                {
                    VideoID = e.Data.Substring(10);
                    ProcessingState = State.Successful;
                }

                // update progress if given
                else if (e.Data.StartsWith("progress"))
                {
                    ProgressString = e.Data.Substring(10);
                    if (!ToolTipMessage.Contains("progress:"))
                    {
                        if (!string.IsNullOrEmpty(ToolTipMessage))
                            ToolTipMessage += "\r\n";
                        ToolTipMessage += $"nose detection progress: {ProgressString}";
                    }
                    else
                    {
                        string pattern = "\\d+/\\d+";
                        ToolTipMessage = Regex.Replace(ToolTipMessage, pattern, ProgressString);
                    }
                }

                // determine success
                else if (e.Data.StartsWith("success"))
                    ProcessingState = State.Successful;

                // get errors and messages
                else
                {
                    string errorMessage = null;
                    if (e.Data.StartsWith("error"))
                        errorMessage = e.Data.Substring(7);
                    if (e.Data.StartsWith("message"))
                        errorMessage = e.Data.Substring(9);
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        if (ProcessingState != State.ExtractVideo)
                            ToolTipMessage += "\r\n";
                        ToolTipMessage += $"{ProcessingState}: {errorMessage}";
                    }
                }

                Console.WriteLine($"o: {e.Data}");
            }
        }

        #region startStop

        private CancellationTokenSource cancellationToken;

        public Task Start(Action<object> action, string videoName, SemaphoreSlim semaphore)
        {
            cancellationToken = new CancellationTokenSource();
            return Task.Factory.StartNew(() =>
            {
                semaphore.Wait();

                try
                {
                    action(videoName);
                }
                finally
                {
                    _ = semaphore.Release();
                }
            }, cancellationToken.Token);
        }

        public void Stop()
        {
            if (cancellationToken != null)
                cancellationToken.Cancel();
        }

        #endregion startStop

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            //if (name == "ProcessingState" || name == "ProgressString")
            //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Progress"));
        }

        #region progress

        private string progress = null;

        public double Progress
        {
            get
            {
                switch (ProcessingState)
                {
                    case State.ExtractVideo:
                        return 1;

                    case State.FindRatPath:
                        if (string.IsNullOrEmpty(ProgressString))
                            return 1;
                        return 1 + 2 * double.Parse(ProgressString.Split('/')[0]) / double.Parse(ProgressString.Split('/')[1]);

                    case State.FindRatFeatues:
                        return 3;

                    case State.SaveToDataBase:
                        return 4;

                    case State.Successful:
                    case State.Failed:
                        return 5;

                    case State.Waiting:
                    default:
                        return 0;
                }
            }
        }

        private string ProgressString
        {
            get => progress;
            set
            {
                progress = value;
                OnPropertyChanged();
                OnPropertyChanged("Progress");
            }
        }

        #endregion progress

        #region processingState

        private State processingState;

        public enum State
        { Waiting, ExtractVideo, FindRatPath, FindRatFeatues, SaveToDataBase, Successful, Failed };

        public State ProcessingState
        {
            get => processingState;
            set
            {
                processingState = value;
                OnPropertyChanged();
                OnPropertyChanged("Progress");
            }
        }

        public bool HasFailed()
        {
            return ProcessingState == State.Failed;
        }

        public bool HasTerminated()
        {
            return ProcessingState == State.Failed || ProcessingState == State.Successful;
        }

        #endregion processingState

        #region reducedName

        private string reducedName;

        public string ReducedName
        {
            get => reducedName;
            set
            {
                reducedName = value;
                OnPropertyChanged();
            }
        }

        #endregion reducedName

        #region videoID

        private string videoID;

        public string VideoID
        {
            get => videoID;
            set
            {
                videoID = value;
                OnPropertyChanged();
            }
        }

        #endregion videoID

        #region videoItem

        private Video videoItem;

        public Video VideoItem
        {
            get => videoItem;
            set
            {
                videoItem = value;
                OnPropertyChanged();
            }
        }

        #endregion videoItem

        #region toolTipMessage

        private string toolTipMessage = "";

        public string ToolTipMessage
        {
            get => toolTipMessage;
            set
            {
                toolTipMessage = value;
                OnPropertyChanged();
            }
        }

        #endregion toolTipMessage
    }
}