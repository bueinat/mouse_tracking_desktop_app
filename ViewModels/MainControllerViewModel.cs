using mouse_tracking_web_app.UtilTypes;
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

        public string VM_CSVString => model.CSVString;

        public bool VM_DragEnabled => model.DragEnabled;

        public string VM_FileExplorerDirectory => model.WorkingPath;

        public bool VM_OverrideDB => model.OverrideDB;

        public DisplayableVideo VM_SelectedVideo
        {
            get => model.SelectedVideo;
            set
            {
                model.SelectedVideo = value;
                NotifyPropertyChanged("VM_SelectedVideo");
            }
        }

        public string VM_VideosPath
        {
            get => model.VideosPath;
            set
            {
                model.VideosPath = value;
                NotifyPropertyChanged("VM_VideosPath");
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}