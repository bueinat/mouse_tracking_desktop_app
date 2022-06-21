using mouse_tracking_web_app.DataBase;
using System.ComponentModel;

namespace mouse_tracking_web_app.ViewModels
{
    public class MainControllerViewModel : INotifyPropertyChanged
    {
        private readonly Models.MainControllerModel model;

        public MainControllerViewModel(Models.MainControllerModel mainController, NavigationTreeViewModel NTVM)
        {
            model = mainController;
            this.NTVM = NTVM;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("VM_" + e.PropertyName);
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public NavigationTreeViewModel NTVM { get; internal set; }

        //public BindingList<DisplayableVideo> VM_DispVideosCollection => model.DispVideosCollection;

        public string VM_VideosPath
        {
            get => model.VideosPath;
            set
            {
                model.VideosPath = value;
                NotifyPropertyChanged("VM_VideosPath");
            }
        }

        public string VM_CSVString => model.CSVString;
        public bool VM_DragEnabled => model.DragEnabled;

        public DisplayableVideo VM_SelectedVideo
        {
            get => model.SelectedVideo;
            set
            {
                model.SelectedVideo = value;
                NotifyPropertyChanged("VM_SelectedVideo");
            }
        }

        public bool VM_OverrideInDB
        {
            get => model.OverrideInDB;
            set => model.OverrideInDB = value;
        }

        //public string VM_ErrorMessage
        //{
        //    get => model.ErrorMessage;
        //    set => model.ErrorMessage = value;
        //}

        public string VM_FileExplorerDirectory
        {
            get => model.FileExplorerDirectory;
            set => model.FileExplorerDirectory = value;
        }

        //public bool VM_HasErrorMessage => model.HasErrorMessage;

        //public bool VM_IsLoading
        //{
        //    get => model.IsLoading;
        //    set => model.IsLoading = value;
        //}

        //public string VM_VideoName
        //{
        //    get => model.VideoName;
        //    set => model.VideoName = value;
        //}

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}