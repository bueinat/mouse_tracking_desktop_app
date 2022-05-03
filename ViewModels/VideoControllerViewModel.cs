using mouse_tracking_web_app.Models;
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
        public string VMVC_FramePath => Model.VC_FramePath;
        public bool VMVC_IsDrinking => Model.VC_IsDrinking;
        public bool VMVC_IsNoseCasting => Model.VC_IsNoseCasting;
        public bool VMVC_IsSniffing => Model.VC_IsSniffing;
        public bool VMVC_IsVideoLoaded => Model.VC_IsVideoLoaded;
        public int VMVC_NFrames => Model.VC_NFrames;

        public bool VMVC_FeaturesPanelFlag
        {
            get => Model.VC_FeaturesPanelFlag;
            set => Model.VC_FeaturesPanelFlag = value;
        }

        public bool VMVC_Pause
        {
            get => Model.VC_Pause;
            set => Model.VC_Pause = value;
        }

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

        public int VMVC_TimeStep => Model.VC_TimeStep;
        public float VMVC_VelocityX => Model.VC_VelocityX;
        public float VMVC_VelocityY => Model.VC_VelocityY;
        public float VMVC_X => Model.VC_X;
        public float VMVC_Y => Model.VC_Y;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}