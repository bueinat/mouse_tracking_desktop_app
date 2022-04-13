using System.ComponentModel;
using System.IO;
using System.Threading;

namespace mouse_tracking_web_app.Models
{
    public class VideoControllerModel : INotifyPropertyChanged
    {
        private readonly MainControllerModel model;
        private int frameNum = 0;
        private readonly float baseSpeed = 1000 / 45;

        public int VC_FrameNum
        {
            get => frameNum;
            set
            {
                frameNum = value;
                NotifyPropertyChanged("VC_FrameNum");
                NotifyPropertyChanged("VC_FramePath");
            }
        }

        public double Speed { get; set; }

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

        public event PropertyChangedEventHandler PropertyChanged;

        public string VC_VideoPath
        {
            get => model.VideoPath;
            set
            {
                model.VideoPath = value;
                VC_FrameNum = 0;
                NotifyPropertyChanged("VC_VideoPath");
                NotifyPropertyChanged("VC_FramesPath");
                NotifyPropertyChanged("VC_FrameNum");
            }
        }

        //public string VC_FramePath
        //{
        //    get => model.FramePath;
        //    set {
        //        model.FramePath = value;
        //        NotifyPropertyChanged("VC_FramePath");
        //    }
        //}

        public int VC_FramesNumber
        {
            get => model.FramesNumber;
            set
            {
                model.FramesNumber = value;
                NotifyPropertyChanged("VC_FramesNumber");
            }
        }

        public string VC_FramesPath => VC_VideoPath + "\\frames";
        public string VC_FramePath => $"{VC_FramesPath}\\frame{VC_FrameNum}.jpg";

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
                        //if (File.Exists(VC_FramePath))
                        if (VC_FrameNum < VC_FramesNumber - 1)
                            VC_FrameNum++;

                        Thread.Sleep((int)(baseSpeed / Speed));// read the data in 10Hz
                    }
                }
            }).Start();
        }
    }
}