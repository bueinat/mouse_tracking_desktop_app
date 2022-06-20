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
        public Utils.ObservableDictionary<string, DisplayableVideo> VM_DisplayableVideos => model.DisplayableVideos;

        public BindingList<DisplayableVideo> VM_DispVideosCollection => model.DispVideosCollection;

        public string VM_CSVString => model.CSVString;
        public bool VM_DragEnabled => model.DragEnabled;

        public bool VM_OverrideInDB
        {
            get => model.OverrideInDB;
            set => model.OverrideInDB = value;
        }

        public string VM_VideosPath
        {
            get => model.VideosPath;
            set => model.VideosPath = value;
        }

        public string VM_ErrorMessage
        {
            get => model.ErrorMessage;
            set => model.ErrorMessage = value;
        }

        public string VM_FileExplorerDirectory
        {
            get => model.FileExplorerDirectory;
            set => model.FileExplorerDirectory = value;
        }

        public bool VM_HasErrorMessage => model.HasErrorMessage;

        public bool VM_IsLoading
        {
            get => model.IsLoading;
            set => model.IsLoading = value;
        }

        //public VideoControllerViewModel VM_VideoControllerViewModel { get; set; }

        public string VM_VideoName
        {
            get => model.VideoName;
            set => model.VideoName = value;
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}