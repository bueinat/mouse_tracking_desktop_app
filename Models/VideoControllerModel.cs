﻿using mouse_tracking_web_app.DataBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace mouse_tracking_web_app.Models
{
    public class VideoControllerModel : INotifyPropertyChanged
    {
        private readonly float baseSpeed = 1000 / 45;
        private readonly MainControllerModel model;
        private bool fPanel = false;
        private string framePath;
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
        public bool VC_DragEnabled => model.DragEnabled;
        public Dictionary<string, bool> VC_Features => VC_VideoAnalysis?.Features[VC_StepCounter];
        public List<string> VC_FeaturesList => model.SM.FeaturesList;

        public bool VC_FeaturesPanelFlag
        {
            get => fPanel;
            set
            {
                fPanel = value;
                NotifyPropertyChanged("VC_FeaturesPanelFlag");
            }
        }

        public AnalysisStats VC_VideoStats => model.VideoStats;

        public Dictionary<string, List<Tuple<int, int>>> VC_FeaturesTimeRanges => VC_VideoStats?.FeaturesTimes;

        public string VC_FramePath
        {
            get => framePath;
            set
            {
                framePath = value;
                NotifyPropertyChanged("VC_FramePath");
            }
        }

        public int VC_NFeatures => model.SM.FeaturesList.Count;

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
                VC_FramePath = VC_VideoAnalysis?.Path[VC_StepCounter].Replace("@WORKING_PATH", model.CachePath);

                NotifyPropertyChanged("VC_StepCounter");
                NotifyPropertyChanged("VC_TimeStep");
                NotifyPropertyChanged("VC_X");
                NotifyPropertyChanged("VC_Y");
                NotifyPropertyChanged("VC_VelocityX");
                NotifyPropertyChanged("VC_VelocityY");
                NotifyPropertyChanged("VC_AccelerationX");
                NotifyPropertyChanged("VC_AccelerationY");
                NotifyPropertyChanged("VC_Curviness");
                NotifyPropertyChanged("VC_Features");
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
                VC_NFrames = VC_VideoAnalysis.TimeStep.Count;
                VC_StepCounter = 0;
                NotifyPropertyChanged("VC_VideoAnalysis");
                NotifyPropertyChanged("VC_FeaturesTimeRanges");
            }
        }

        public string VC_VideoName => model.VideoName;
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

        //private ObjectId _id = default(ObjectId);
        //private ObjectId? _id = null;

        public bool InitializeVideo(string videoID)
        {
            if (!string.IsNullOrEmpty(videoID))
            {
                Video video = model.DBHandler.GetVideoByID(videoID);
                if (!(video is null))
                {
                    VC_VideoAnalysis = model.DBHandler.GetAnalysisByID(video.Analysis);
                    VC_StepCounter = 0;
                }
                return true;
            }
            return false;
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

                        Thread.Sleep((int)(baseSpeed / VC_Speed));
                    }
                }
            }).Start();
        }
    }
}