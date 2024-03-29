﻿using mouse_tracking_web_app.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace mouse_tracking_web_app.ViewModels
{
    public class VideoControllerViewModel : INotifyPropertyChanged
    {
        public VideoControllerViewModel(VideoControllerModel model)
        {
            Model = model;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("VM" + e.PropertyName);
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public VideoControllerModel Model { get; }
        public float VMVC_AccelerationX => Model.VC_AccelerationX;
        public float VMVC_AccelerationY => Model.VC_AccelerationY;
        public float VMVC_Curviness => Model.VC_Curviness;
        public bool VMVC_DragEnabled => Model.VC_DragEnabled;
        public Dictionary<string, bool> VMVC_Features => Model.VC_Features;
        public List<string> VMVC_FeaturesList => Model.VC_FeaturesList;

        public bool VMVC_FeaturesPanelFlag
        {
            get => Model.VC_FeaturesPanelFlag;
            set => Model.VC_FeaturesPanelFlag = value;
        }

        public Dictionary<string, List<Tuple<int, int>>> VMVC_FeaturesTimeRanges => Model.VC_FeaturesTimeRanges;
        public string VMVC_FramePath => Model.VC_FramePath;
        public int VMVC_NFeatures => Model.VC_NFeatures;

        //
        public int VMVC_NFrames => Model.VC_NFrames;

        public bool VMVC_Pause
        {
            get => Model.VC_Pause;
            set => Model.VC_Pause = value;
        }

        public Tuple<float, float> VMVC_Position => new Tuple<float, float>(VMVC_X, VMVC_Y);

        public double VMVC_Speed
        {
            get => Model.VC_Speed;
            set => Model.VC_Speed = value;
        }

        public int VMVC_StepCounter
        {
            get => Model.VC_StepCounter;
            set => Model.VC_StepCounter = value;
        }

        public bool VMVC_Stop
        {
            get => Model.VC_Stop;
            set
            {
                Model.VC_Stop = value;
                NotifyPropertyChanged("VMVC_Stop");
            }
        }

        public int VMVC_TimeStep => Model.VC_TimeStep;
        public float VMVC_VelocityX => Model.VC_VelocityX;
        public float VMVC_VelocityY => Model.VC_VelocityY;
        public DataBase.Analysis VMVC_VideoAnalysis => Model.VC_VideoAnalysis;
        public string VMVC_VideoName => Model.VC_VideoName;
        public float VMVC_X => Model.VC_X;
        public float VMVC_Y => Model.VC_Y;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}