using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.Models
{
    public class MainControllerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public DataBaseHandler DBHandler { get; }

        public MainControllerModel()
        {
            DBHandler = new DataBaseHandler(this);
        }
    }
}
