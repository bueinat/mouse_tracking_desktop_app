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
        private int framesNumber = 1;
        private bool isLoading = false;
        private bool pause = true;
        private string videoName = "";
        private string videoPath;
        private string videoID;

        public MainControllerModel()
        {
            DBHandler = new DataBase.DataBaseHandler(this);
            DBHandler.Connect();
            CodeRunner = new OuterCodeRunner(this);
            VC = new VideoControllerModel(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //public string VideoID
        //{
        //    get => videoID;
        //    set
        //    {
        //        videoID = value;
        //        NotifyPropertyChanged("VideoID");
        //    }
        //}
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

        public bool IsVideoLoaded => !string.IsNullOrEmpty(VideoName);

        public bool IsLoading
        {
            get => isLoading;
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

        public string VideoPath
        {
            get => videoPath;
            set
            {
                videoPath = value;
                NotifyPropertyChanged("VideoPath");
                NotifyPropertyChanged("IsVideoLoaded");
            }
        }

        public string ArchivePath { get; set; }

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
                if (rawResult[i].StartsWith("archive path"))
                    result["ArchivePath"] = rawResult[i].Substring(13);
                if (rawResult[i].StartsWith("nframes"))
                    result["FramesNumber"] = rawResult[i].Substring(9);
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
            if (processedResult.ContainsKey("VideoPath"))
                VideoPath = processedResult["VideoPath"];
            if (processedResult.ContainsKey("FramesNumber"))
                FramesNumber = int.Parse(processedResult["FramesNumber"]);
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