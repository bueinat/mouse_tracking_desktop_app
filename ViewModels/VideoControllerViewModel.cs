using mouse_tracking_web_app.Models;
using System.ComponentModel;

namespace mouse_tracking_web_app.ViewModels
{
    public class VideoControllerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public VideoControllerModel Model { get; }

        public VideoControllerViewModel(VideoControllerModel model)
        {
            Model = model;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("VM_" + e.PropertyName);
            };
        }

        public string VM_VC_FramePath
        {
            get => Model.VC_FramePath;
            //set => Model.VC_FramePath = value;
        }

        public int VM_VC_FramesNumber
        {
            get => Model.VC_FramesNumber;
            set => Model.VC_FramesNumber = value;
        }

        public int VM_VC_FrameNum
        {
            get => Model.VC_FrameNum;
            set => Model.VC_FrameNum = value;
        }

        public string VM_VC_FramesPath
        {
            get => Model.VC_FramesPath;
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}