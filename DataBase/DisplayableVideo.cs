using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.DataBase
{
    public class DisplayableVideo : INotifyPropertyChanged
    {
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
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

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