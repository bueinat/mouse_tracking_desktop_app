using System.Collections.Generic;

using System.ComponentModel;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.Models
{
    public class MainControllerModel : INotifyPropertyChanged
    {
        private bool isLoading = false;

        private string videoName;

        public MainControllerModel()
        {
            DBHandler = new DataBase.DataBaseHandler(this);
            CodeRunner = new OuterCodeRunner(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public OuterCodeRunner CodeRunner { get; }

        public DataBase.DataBaseHandler DBHandler { get; }

        public bool IsLoading
        {
            get
            {
                return isLoading;
            }
            set
            {
                isLoading = value;
                NotifyPropertyChanged("IsLoading");
            }
        }
        public string VideoName
        {
            get
            {
                return videoName;
            }
            set
            {
                videoName = value;
                ProcessVideo(value);
            }
        }
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task ProcessVideo(string videoPath)
        {
            IsLoading = true;
            await Task.Delay(2000);
            string script = @"OutsideCode\ProcessVideoScript.py";
            List<string> argv = new List<string>
            {
                videoPath
            };
            string result = await CodeRunner.RunCmd(script, argv);
            //task.Wait();
            //string result = task.Result;
            //return result;

            IsLoading = false;
        }
    }
}