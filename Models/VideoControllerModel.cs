using mouse_tracking_web_app.DataBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Threading;

namespace mouse_tracking_web_app.Models
{
    public class VideoControllerModel : INotifyPropertyChanged
    {
        private readonly float baseSpeed = 1000 / 45;
        private readonly MainControllerModel model;
        private bool fPanel = false;
        private string framePath = "/Images/default_image.png";
        private int nframes = 1;
        private double speed;
        private int stepCounter;

        public VideoControllerModel(MainControllerModel model)
        {
            this.model = model;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("VC_" + e.PropertyName);
            };
            VC_Stop = true;
            VC_Speed = 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public float VC_AccelerationX => (VC_VideoAnalysis is null) ? 0 : VC_VideoAnalysis.AccelerationX[VC_StepCounter];

        public float VC_AccelerationY => (VC_VideoAnalysis is null) ? 0 : VC_VideoAnalysis.AccelerationY[VC_StepCounter];

        public float VC_Curviness => (VC_VideoAnalysis is null) ? 0 : VC_VideoAnalysis.Curviness[VC_StepCounter];

        public List<string> VC_FeaturesList => new List<string>(ConfigurationManager.AppSettings["FeaturesList"].Split(','));

        public bool VC_FeaturesPanelFlag
        {
            get => fPanel;
            set
            {
                fPanel = value;
                NotifyPropertyChanged("VC_FeaturesPanelFlag");
            }
        }

        public Dictionary<string, List<Tuple<int, int>>> VC_FeaturesTimeRanges => VC_VideoAnalysis?.GetFeaturesTimes();

        public string VC_FramePath
        {
            get => framePath;
            set
            {
                framePath = value;
                NotifyPropertyChanged("VC_FramePath");
            }
        }

        public bool VC_IsDrinking => !(VC_VideoAnalysis is null) && VC_VideoAnalysis.IsDrinking[VC_StepCounter];

        public bool VC_IsNoseCasting => !(VC_VideoAnalysis is null) && VC_VideoAnalysis.IsNoseCasting[VC_StepCounter];

        public bool VC_IsSniffing => !(VC_VideoAnalysis is null) && VC_VideoAnalysis.IsSniffing[VC_StepCounter];

        public bool VC_IsVideoLoaded => model.IsVideoLoaded;

        public int VC_NFeatures => VC_FeaturesList.Count;

        public int VC_NFrames
        {
            get => nframes;
            set
            {
                nframes = value;
                NotifyPropertyChanged("VC_NFrames");
            }
        }

        public bool VC_Pause
        {
            get => model.Pause;
            set
            {
                model.Pause = value;
                NotifyPropertyChanged("VC_Pause");
            }
        }

        public double VC_Speed
        {
            get => speed;
            set
            {
                speed = value;
                NotifyPropertyChanged("VC_Speed");
            }
        }

        public int VC_StepCounter
        {
            get => stepCounter;
            set
            {
                stepCounter = value;
                VC_FramePath = VC_VideoAnalysis?.Path[VC_StepCounter].Replace("@WORKING_PATH", model.ArchivePath);

                NotifyPropertyChanged("VC_StepCounter");
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

        public bool VC_Stop
        {
            get => model.Stop;
            set
            {
                model.Stop = value;
                NotifyPropertyChanged("VC_Stop");
            }
        }

        public int VC_TimeStep => (VC_VideoAnalysis is null) ? 0 : VC_VideoAnalysis.TimeStep[VC_StepCounter];

        public float VC_VelocityX => (VC_VideoAnalysis is null) ? 0 : VC_VideoAnalysis.VelocityX[VC_StepCounter];

        public float VC_VelocityY => (VC_VideoAnalysis is null) ? 0 : VC_VideoAnalysis.VelocityY[VC_StepCounter];

        public Analysis VC_VideoAnalysis
        {
            get => model.VideoAnalysis;
            set
            {
                model.VideoAnalysis = value;
                VC_NFrames = VC_VideoAnalysis.TimeStep.Count - 1;
                VC_StepCounter = 0;
                NotifyPropertyChanged("VC_VideoAnalysis");
                NotifyPropertyChanged("VC_FeaturesTimeRanges");
            }
        }

        public float VC_X => (VC_VideoAnalysis is null) ? 0 : VC_VideoAnalysis.X[VC_StepCounter];

        public float VC_Y => (VC_VideoAnalysis is null) ? 0 : VC_VideoAnalysis.Y[VC_StepCounter];

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

        public void InitializeVideo(string videoID)
        {
            Video video = model.DBHandler.GetVideoByID(videoID);
            VC_VideoAnalysis = model.DBHandler.GetAnalysisByID(video.Analysis);
            VC_StepCounter = 0;
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
                        if (VC_StepCounter < VC_NFrames)
                            VC_StepCounter++;
                        else
                            VC_Pause = true;

                        Thread.Sleep((int)(baseSpeed / VC_Speed));
                    }
                }
            }).Start();
        }
    }
}