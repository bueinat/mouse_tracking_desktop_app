using mouse_tracking_web_app.DataBase;
using System.ComponentModel;
using System.Threading;

namespace mouse_tracking_web_app.Models
{
    public class VideoControllerModel : INotifyPropertyChanged
    {
        private readonly float baseSpeed = 1000 / 45;
        private readonly MainControllerModel model;
        private string framePath = "../Images/default_image.png";
        private int nframes = 1;
        private int frameNum = 0;
        private string videoPath;
        private DataBase.Analysis analysisData;
        private int stepCounter;

        public int VC_StepCounter
        {
            get => stepCounter;
            set
            {
                stepCounter = value;
                NotifyPropertyChanged("VC_StepCounter");
                NotifyPropertyChanged("VC_TimeStep");
                NotifyPropertyChanged("VC_X");
                NotifyPropertyChanged("VC_Y");
                NotifyPropertyChanged("VC_VelocityX");
                NotifyPropertyChanged("VC_VelocityY");
                NotifyPropertyChanged("VC_AccelerationX");
                NotifyPropertyChanged("VC_AccelerationY");
                NotifyPropertyChanged("VC_Curviness");

                VC_FrameNum = ConvertFramePathToNum(VC_Analysis.Path[VC_StepCounter]);
                VC_FramePath = VC_Analysis.Path[VC_StepCounter].Replace("@WORKING_PATH", model.ArchivePath);
                NotifyPropertyChanged("VC_IsSniffing");
                NotifyPropertyChanged("VC_IsDrinking");
                NotifyPropertyChanged("VC_IsNoseCasting");
            }
        }

        public bool VC_IsVideoLoaded => model.IsVideoLoaded;

        public VideoControllerModel(MainControllerModel model)
        {
            this.model = model;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("VC_" + e.PropertyName);
            };
            VC_Stop = true;
            Speed = 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public double Speed { get; set; }

        public DataBase.Analysis VC_Analysis
        {
            get => analysisData;
            set
            {
                analysisData = value;
                NotifyPropertyChanged("VC_Analysis");
                VC_NFrames = VC_Analysis.TimeStep.Count;
                //NotifyPropertyChanged("VC_Nframes");

                VC_FrameNum = ConvertFramePathToNum(VC_Analysis.Path[VC_StepCounter]);
                VC_FramePath = VC_Analysis.Path[VC_StepCounter].Replace("@WORKING_PATH", model.ArchivePath);

                NotifyPropertyChanged("VC_FrameNum");
                NotifyPropertyChanged("VC_TimeStep");
                NotifyPropertyChanged("VC_X");
                NotifyPropertyChanged("VC_Y");
                NotifyPropertyChanged("VC_VelocityX");
                NotifyPropertyChanged("VC_VelocityY");
                NotifyPropertyChanged("VC_AccelerationX");
                NotifyPropertyChanged("VC_AccelerationY");
                NotifyPropertyChanged("VC_Curviness");
                NotifyPropertyChanged("VC_IsSniffing");
                NotifyPropertyChanged("VC_IsDrinking");
                NotifyPropertyChanged("VC_IsNoseCasting");
            }
        }

        public int ConvertFramePathToNum(string framePath)
        {
            string[] subs = framePath.Split('\\');
            if (subs.Length > 0)
            {
                string fileName = subs[subs.Length - 1];
                return int.Parse(fileName.Split('.')[0].Substring(5));
            }
            return 0;
        }

        public int VC_FrameNum
        {
            get => frameNum;
            set
            {
                frameNum = value;
                NotifyPropertyChanged("VC_FrameNum");
            }
        }

        public int VC_TimeStep => (VC_Analysis is null) ? 0 : VC_Analysis.TimeStep[VC_StepCounter];
        public float VC_X => (VC_Analysis is null) ? 0 : VC_Analysis.X[VC_StepCounter];
        public float VC_Y => (VC_Analysis is null) ? 0 : VC_Analysis.Y[VC_StepCounter];
        public float VC_VelocityX => (VC_Analysis is null) ? 0 : VC_Analysis.VelocityX[VC_StepCounter];
        public float VC_VelocityY => (VC_Analysis is null) ? 0 : VC_Analysis.VelocityY[VC_StepCounter];
        public float VC_AccelerationX => (VC_Analysis is null) ? 0 : VC_Analysis.AccelerationX[VC_StepCounter];
        public float VC_AccelerationY => (VC_Analysis is null) ? 0 : VC_Analysis.AccelerationY[VC_StepCounter];
        public float VC_Curviness => (VC_Analysis is null) ? 0 : VC_Analysis.Curviness[VC_StepCounter];

        public string VC_FramePath
        {
            get => framePath;
            set
            {
                framePath = value;
                NotifyPropertyChanged("VC_FramePath");
            }
        }

        public bool VC_IsSniffing => !(VC_Analysis is null) && VC_Analysis.IsSniffing[VC_StepCounter];
        public bool VC_IsDrinking => !(VC_Analysis is null) && VC_Analysis.IsDrinking[VC_StepCounter];
        public bool VC_IsNoseCasting => !(VC_Analysis is null) && VC_Analysis.IsNoseCasting[VC_StepCounter];

        //public int VC_NFrames => VC_Analysis.TimeStep.Count;
        public int VC_NFrames
        {
            get => nframes;
            set
            {
                nframes = value;
                NotifyPropertyChanged("VC_NFrames");
            }
        }

        public string VC_FramesPath => VC_VideoPath + "\\frames";

        public bool VC_Pause
        {
            get => model.Pause;
            set
            {
                model.Pause = value;
                NotifyPropertyChanged("VC_Pause");
            }
        }

        public bool VC_Stop
        {
            get => model.Stop;
            set
            {
                model.Stop = value;
                NotifyPropertyChanged("VC_Stop");
            }
        }

        public string VC_VideoPath
        {
            get => videoPath;
            set
            {
                videoPath = value;
                //VC_FrameNum = 0;
                NotifyPropertyChanged("VC_VideoPath");
                NotifyPropertyChanged("VC_FramesPath");
                //NotifyPropertyChanged("VC_FrameNum");
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Run()
        {
            if (VC_Stop)
            {
                VC_Stop = false;
                RunWrapped();
            }
        }

        public void RunWrapped()
        {
            new Thread(delegate ()
            {
                while (!VC_Stop)
                {
                    if (!VC_Pause)
                    {
                        if (VC_StepCounter < VC_NFrames - 1)
                            VC_StepCounter++;
                        else
                            VC_Pause = true;

                        Thread.Sleep((int)(baseSpeed / Speed));
                    }
                }
            }).Start();
        }

        public void InitializeVideo(string videoID)
        {
            Video video = model.DBHandler.GetVideoByID(videoID);
            VC_Analysis = model.DBHandler.GetAnalysisByID(video.Analysis);
            VC_VideoPath = video.LinkToData.Replace("@WORKING_PATH", ".");
            VC_StepCounter = 0;
        }
    }
}