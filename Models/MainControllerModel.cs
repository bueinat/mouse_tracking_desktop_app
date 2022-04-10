using System.Collections.Generic;

using System.ComponentModel;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.Models
{
    public class MainControllerModel : INotifyPropertyChanged
    {
        // TODO: unite all frames stuff into one class / dictionary
        private string errorMessage = "";
        private string framePath = "../Images/default_image.png";
        private bool isLoading = false;
        private bool pause;
        private string videoName;
        private string videoPath;
        private int framesNumber = 0;
        public MainControllerModel()
        {
            DBHandler = new DataBase.DataBaseHandler(this);
            CodeRunner = new OuterCodeRunner(this);
            FI = new FramesIterator(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public OuterCodeRunner CodeRunner { get; }

        public DataBase.DataBaseHandler DBHandler { get; }

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                NotifyPropertyChanged("ErrorMessage");
                NotifyPropertyChanged("HasErrorMessage");
            }
        }

        public FramesIterator FI { get; }

        public string FramePath
        {
            get => framePath;
            set
            {
                framePath = value;
                NotifyPropertyChanged("FramePath");
            }
        }

        public int FramesNumber
        {
            get => framesNumber;
            set
            {
                framesNumber = value;
                NotifyPropertyChanged("FramesNumber");
            }
        }

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

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
        public bool Pause
        {
            get => pause;
            set
            {
                pause = value;
                NotifyPropertyChanged("pause");
            }
        }

        public bool Stop { get; set; }

        public string VideoName
        {
            get => videoName;
            set
            {
                videoName = value;
                NotifyPropertyChanged("VideoName");
                _ = ProcessVideo(value);
            }
        }

        public string VideoPath
        {
            get => videoPath;
            set
            {
                videoPath = value;
                NotifyPropertyChanged("VideoPath");
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Dictionary<string, string> ProcessResult(string[] rawResult)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            for (int i = 0; i < rawResult.Length; i++)
            {
                if (rawResult[i].StartsWith("e"))
                    result["ErrorMessage"] = rawResult[i].Substring(7);
                if (rawResult[i].StartsWith("video path"))
                    result["VideoPath"] = rawResult[i].Substring(12);
                if (rawResult[i].StartsWith("nframes"))
                    result["FramesNumber"] = rawResult[i].Substring(9);
            }
            return result;
        }

        public async Task ProcessVideo(string videoPath)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            //await Task.Delay(2000);
            string script = @"OutsideCode\ProcessVideoScript.py";
            List<string> argv = new List<string>
            {
                videoPath
            };
            string[] rawResult = await CodeRunner.RunCmd(script, argv);
            Dictionary<string, string> processedResult = ProcessResult(rawResult);
            if (processedResult.ContainsKey("ErrorMessage"))
                ErrorMessage = processedResult["ErrorMessage"];
            if (processedResult.ContainsKey("VideoPath"))
                VideoPath = processedResult["VideoPath"];
            if (processedResult.ContainsKey("FramesNumber"))
                FramesNumber = int.Parse(processedResult["FramesNumber"]);
            IsLoading = false;
            FI.Run();
        }
        public void StopMethod()
        {
            Stop = true;
        }
    }
}