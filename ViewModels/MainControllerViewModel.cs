using System.ComponentModel;

namespace mouse_tracking_web_app.ViewModels
{
    public class MainControllerViewModel : INotifyPropertyChanged
    {
        private Models.MainControllerModel model;

        public event PropertyChangedEventHandler PropertyChanged;

        public string VM_VideoName
        {
            //get { return model.VideoName; }
            set { model.VideoName = value; }
        }

        public MainControllerViewModel(Models.MainControllerModel mainController)
        {
            this.model = mainController;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("VM_" + e.PropertyName);
            };
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Connect()
        {
            model.DBHandler.Connect();
        }
    }
}