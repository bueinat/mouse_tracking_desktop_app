using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace mouse_tracking_web_app.DataBase
{
    public class DisplayableVideo : INotifyPropertyChanged
    {
        private State processingState;
        private string reducedName;
        private string videoID;
        private Video videoItem;

        public event PropertyChangedEventHandler PropertyChanged;

        public enum State
        { ExtractVideo, FindRatPath, FindRatFeatues, SaveToDataBase, Successful, Failed };

        public State ProcessingState
        {
            get => processingState;
            set
            {
                processingState = value;
                OnPropertyChanged();
            }
        }

        public string ReducedName
        {
            get => reducedName;
            set
            {
                reducedName = value;
                OnPropertyChanged();
            }
        }

        public string VideoID
        {
            get => videoID;
            set
            {
                videoID = value;
                OnPropertyChanged();
            }
        }

        public Video VideoItem
        {
            get => videoItem;
            set
            {
                videoItem = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}