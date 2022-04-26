using System.Collections.Generic;

using System.ComponentModel;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.Models
{
    public class MainControllerModel : INotifyPropertyChanged
    {
        private string errorMessage = "";

        private bool isLoading = false;
        private bool pause = true;
        private string videoID;
        private string videoName = "";

        public MainControllerModel()
        {
            DBHandler = new DataBase.DataBaseHandler(this);
            DBHandler.Connect();
            CodeRunner = new OuterCodeRunner();
            VC = new VideoControllerModel(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string ArchivePath { get; set; }
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

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                NotifyPropertyChanged("IsLoading");
            }
        }

        public bool IsVideoLoaded => !string.IsNullOrEmpty(VideoName);

        public bool Pause
        {
            get => pause;
            set
            {
                pause = value;
                NotifyPropertyChanged("Pause");
            }
        }

        public bool Stop { get; set; }
        public VideoControllerModel VC { get; }

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
                //if (rawResult[i].StartsWith("video path"))
                //    result["VideoPath"] = rawResult[i].Substring(12);
                if (rawResult[i].StartsWith("archive path"))
                    result["ArchivePath"] = rawResult[i].Substring(13);
                //if (rawResult[i].StartsWith("nframes"))
                //    result["FramesNumber"] = rawResult[i].Substring(9);
                if (rawResult[i].StartsWith("video id"))
                    result["VideoID"] = rawResult[i].Substring(10);
            }
            return result;
        }

        public async Task ProcessVideo(string videoPath)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            string script = @"OutsideCode\ProcessVideoScript.py";
            List<string> argv = new List<string>
            {
                videoPath
            };
            string[] rawResult = await CodeRunner.RunCmd(script, argv);
            Dictionary<string, string> processedResult = ProcessResult(rawResult);
            if (processedResult.ContainsKey("ErrorMessage"))
                ErrorMessage = processedResult["ErrorMessage"];
            if (processedResult.ContainsKey("VideoID"))
                videoID = processedResult["VideoID"];
            if (processedResult.ContainsKey("ArchivePath"))
                ArchivePath = processedResult["ArchivePath"];
            IsLoading = false;
            VC.InitializeVideo(videoID);
            VC.Run();
        }

        public void StopMethod()
        {
            Stop = true;
        }
    }
}