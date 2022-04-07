using System.Collections.Generic;

using System.ComponentModel;

namespace mouse_tracking_web_app.Models
{
    public class MainControllerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string VideoName
        {
            set => ProcessVideo(value); // this is something like a lambda function in C#
        }

        public DataBase.DataBaseHandler DBHandler { get; }
        public OuterCodeRunner CodeRunner { get; }

        public MainControllerModel()
        {
            DBHandler = new DataBase.DataBaseHandler(this);
            CodeRunner = new OuterCodeRunner(this);
        }

        public void ProcessVideo(string videoPath)
        {
            string script = @"OutsideCode\ProcessVideoScript.py"; // @"C:\Users\buein\Downloads\test_code.py";
            List<string> argv = new List<string>
            {
                videoPath
            };
            _ = CodeRunner.RunCmd(script, argv);
        }
    }
}