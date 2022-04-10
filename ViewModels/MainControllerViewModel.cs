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
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string VM_ErrorMessage
        {
            get
            {
                return model.ErrorMessage;
            }
            set
            {
                model.ErrorMessage = value;
            }
        }

        public bool VM_HasErrorMessage
        {
            get
            {
                return model.HasErrorMessage;
            }
        }

        public string VM_VideoName
        {
            get { return model.VideoName; }
            set => model.VideoName = value;
        }

        public string VM_FramePath
        {
            get { return model.FramePath; }
            set => model.FramePath = value;
        }

        public bool VM_IsLoading
        {
            get => model.IsLoading;
            set => model.IsLoading = value;
        }

        public void Connect()
        {
            model.DBHandler.Connect();
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}