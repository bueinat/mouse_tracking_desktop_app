using System.ComponentModel;
using System.IO;
using System.Threading;

namespace mouse_tracking_web_app.Models
{
    public class FramesIterator : INotifyPropertyChanged
    {
        private readonly MainControllerModel model;
        private int frameNum;
        private readonly float baseSpeed = 1000 / 45;

        public double Speed { get; set; }

        public bool FI_Pause
        {
            get => model.Pause;
            set => model.Pause = value;
        }

        public bool FI_Stop
        {
            get => model.Stop;
            set => model.Stop = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string FI_VideoPath
        {
            get => model.VideoPath;
            set
            {
                model.VideoPath = value;
                frameNum = 0;
            }
        }

        public string FI_FramePath
        {
            get => model.FramePath;
            set
            {
                model.FramePath = value;
            }
        }

        public string FI_FramesPath => FI_VideoPath + "\\frames";

        public FramesIterator(MainControllerModel model)
        {
            this.model = model;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("FI_" + e.PropertyName);
            };
            FI_Stop = true;
            Speed = 1;
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Run()
        {
            if (FI_Stop)
            {
                FI_Stop = false;
                RunWrapped();
            }
        }

        public void RunWrapped()
        {
            new Thread(delegate ()
            {
                while (!FI_Stop)
                {
                    if (!FI_Pause)
                    {
                        // ReadLine(); // this should be changed
                        FI_FramePath = $"{FI_FramesPath}\\frame{frameNum}.jpg";
                        if (File.Exists(FI_FramePath))
                            frameNum++;
                        else
                        {
                            frameNum--;
                            FI_FramePath = $"{FI_FramesPath}\\frame{frameNum}.jpg";
                            FI_Pause = true;
                        }

                        Thread.Sleep((int)(baseSpeed / Speed));// read the data in 10Hz
                    }
                }
            }).Start();
        }
    }
}