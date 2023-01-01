using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.UtilTypes
{
    /// <summary>
    /// Class <c>DisplayableVideo</c> represents a video object which can be processed and diplayed
    /// </summary>
    public class DisplayableVideo : INotifyPropertyChanged
    {
        #region propertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion propertyChanged

        #region handlers

        /// <summary>
        /// Handler <c>ErrorHandler</c> is a <cref name="DataReceivedEventHandler"/>
        /// which is fired when data is written to <c>stderr</c>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ErrorHandler(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                // TODO: use this to update on
                Console.WriteLine($"{ReducedName}, e: {e.Data}");
            }
        }

        /// <summary>
        /// Handler <c>OutputHandler</c> is a <cref name="DataReceivedEventHandler"/>
        /// which is fired when data is written to <c>stdout</c>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                // change state if needed (running states)
                if (e.Data == "FindRatFeatures")
                    ProcessingState = State.FindRatFeatures;
                else if (e.Data == "FindRatPath")
                    ProcessingState = State.FindRatPath;
                else if (e.Data == "SaveToDataBase")
                    ProcessingState = State.SaveToDataBase;

                // get video ID if given
                else if (e.Data.StartsWith("video id"))
                {
                    VideoID = e.Data.Substring(10);
                }

                // update progress if given
                else if (e.Data.StartsWith("progress"))
                {
                    ProgressString = e.Data.Substring(10);
                    if (!ToolTipMessage.Contains("progress:"))
                        AppendToToolTipMessage($"nose detection progress: {ProgressString}");
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
                        AppendToToolTipMessage($"{ProcessingState}: {errorMessage}");
                    else if (ProcessingState == State.Failed)
                        AppendToToolTipMessage($"{ProcessingState}: unknown error occured");
                }

                Console.WriteLine($"{ReducedName}, o: {e.Data}");
            }
        }

        #endregion handlers

        #region startStop

        /// <summary>
        /// Cancellation Token which is used to stop a processing if needed
        /// </summary>
        private CancellationTokenSource _cancellationToken;

        /// <summary>
        /// Method <c>Start</c> runs <paramref name="action"/> on <paramref name="videoName"/>.
        /// </summary>
        /// <param name="action">function which is ran</param>
        /// <param name="videoName">the video on which <paramref name="action"/> runs.</param>
        /// <param name="semaphore">allows to limit parallelism.</param>
        /// <returns>A task to be ran.</returns>
        public Task Start(Action<object, object> action, string videoName)
        {
            _cancellationToken = new CancellationTokenSource();
            return Task.Factory.StartNew(() =>
            {
                ProcessingState = State.ExtractVideo;
                action(videoName, _cancellationToken.Token);
                if (_cancellationToken.IsCancellationRequested)
                    ProcessingState = State.Canceled;
                else if (ProcessingState != State.Successful)
                    ProcessingState = State.Failed;
            }, _cancellationToken.Token);
        }

        /// <summary>
        /// Method <c>Stop</c> is used to stop a running process.
        /// </summary>
        public void Stop()
        {
            if (_cancellationToken != null)
            {
                _cancellationToken.Cancel();
                ProcessingState = State.Canceled;
                AppendToToolTipMessage("process was canceled.");
            }
        }

        #endregion startStop

        #region progress

        private double _progress;
        private string _progressString = null;

        /// <summary>
        /// Property <c>Progress</c> indiates to processing stage numerically.
        /// </summary>
        public double Progress
        {
            get
            {
                switch (ProcessingState)
                {
                    case State.ExtractVideo:
                        _progress = 1;
                        break;

                    case State.FindRatFeatures:
                        _progress = 2;
                        break;

                    case State.FindRatPath:
                        if (string.IsNullOrEmpty(ProgressString))
                        {
                            _progress = 1;
                        }
                        else
                        {
                            double numerator = double.Parse(ProgressString.Split('/')[0]);
                            double denominator = double.Parse(ProgressString.Split('/')[1]);
                            _progress = 1 + (2 * numerator / denominator);
                        }
                        break;

                    case State.SaveToDataBase:
                        _progress = 4;
                        break;

                    case State.Successful:
                    case State.Failed:
                        _progress = 5;
                        break;

                    case State.Waiting:
                        _progress = 0;
                        break;

                    case State.Canceled:
                    default:
                        break;
                }
                return _progress;
            }
        }

        private string ProgressString
        {
            get => _progressString;
            set
            {
                _progressString = value;
                OnPropertyChanged();
                OnPropertyChanged("Progress");
            }
        }

        #endregion progress

        #region processingState

        private State _processingState;

        /// <summary>
        /// Enum <c>State</c> states different stages of processing
        /// </summary>
        public enum State
        { Waiting, ExtractVideo, FindRatFeatures, FindRatPath, SaveToDataBase, Successful, Failed, Canceled };

        /// <summary>
        /// Property <c>ProcessingState</c> states the processing state of the <see cref="DisplayableVideo"/>.
        /// </summary>
        public State ProcessingState
        {
            get => _processingState;
            set
            {
                _processingState = value;
                OnPropertyChanged();
                OnPropertyChanged("Progress");
            }
        }

        #endregion processingState

        #region reducedName

        private string _reducedName;

        /// <summary>
        /// Property <c>ReducedName</c> is the relative name of the <see cref="DisplayableVideo"/>.
        /// </summary>
        public string ReducedName
        {
            get => _reducedName;
            set
            {
                _reducedName = value;
                OnPropertyChanged();
            }
        }

        #endregion reducedName

        #region videoID

        private string _videoID;

        /// <summary>
        /// Property <c>VideoID</c> is the ID of the <see cref="DisplayableVideo"/> as written in the DataBase.
        /// </summary>
        public string VideoID
        {
            get => _videoID;
            set
            {
                _videoID = value;
                OnPropertyChanged();
            }
        }

        #endregion videoID

        #region toolTipMessage

        private string _toolTipMessage = "";

        /// <summary>
        /// Property <c>ToolTipMessage</c> is the message which is shown in the videos list.
        /// </summary>
        public string ToolTipMessage
        {
            get => _toolTipMessage;
            set
            {
                _toolTipMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Method <c>AppendToToolTipMessage</c> wraps appending to  <see cref="ToolTipMessage"/> with adding newline if needed.
        /// </summary>
        /// <param name="message">the string which should be added.</param>
        private void AppendToToolTipMessage(string message)
        {
            if (!string.IsNullOrEmpty(ToolTipMessage))
                ToolTipMessage += "\r\n";
            ToolTipMessage += message;
        }

        #endregion toolTipMessage
    }
}