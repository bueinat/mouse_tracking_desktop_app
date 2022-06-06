using System.ComponentModel;

namespace mouse_tracking_web_app.ViewModels
{
    public class MainControllerViewModel : INotifyPropertyChanged
    {
        private readonly Models.MainControllerModel model;

        public MainControllerViewModel(Models.MainControllerModel mainController)
        {
            model = mainController;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("VM_" + e.PropertyName);
            };
            //VM_VideoControllerViewModel = (Application.Current as App).VCVM;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string VM_CSVString => model.CSVString;

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