using System.ComponentModel;

namespace mouse_tracking_web_app.Models
{
    public class MainControllerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DataBase.DataBaseHandler DBHandler { get; }

        public MainControllerModel()
        {
            DBHandler = new DataBase.DataBaseHandler(this);
        }
    }
}