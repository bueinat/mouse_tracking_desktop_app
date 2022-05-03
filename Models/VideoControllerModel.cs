using mouse_tracking_web_app.DataBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Media;

namespace mouse_tracking_web_app.Models
{
    public class VideoControllerModel : INotifyPropertyChanged
    {
        private readonly float baseSpeed = 1000 / 45;
        private readonly MainControllerModel model;
        private Analysis analysisData;
        private string framePath = "../Images/default_image.png";
        private int nframes = 1;
        private int stepCounter;
        private double speed;
        private bool fPanel = false;

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

        public double VC_Speed
        {
            get => speed;
            set
            {
                speed = value;
                NotifyPropertyChanged("VC_Speed");
            }
        }

        public bool VC_FeaturesPanelFlag
        {
            get => fPanel;
            set
            {
                fPanel = value;
                NotifyPropertyChanged("VC_FeaturesPanelFlag");
            }
        }

        public float VC_AccelerationX => (VC_Analysis is null) ? 0 : VC_Analysis.AccelerationX[VC_StepCounter];

        public float VC_AccelerationY => (VC_Analysis is null) ? 0 : VC_Analysis.AccelerationY[VC_StepCounter];

        public Analysis VC_Analysis
        {
            get => analysisData;
            set
            {
                analysisData = value;
                VC_NFrames = VC_Analysis.TimeStep.Count - 1;
                VC_StepCounter = 0;
                NotifyPropertyChanged("VC_Analysis");
                NotifyPropertyChanged("VC_FeaturesTimeRanges");
                NotifyPropertyChanged("VC_ColorRanges");
                var a = VC_ColorRanges;
            }
        }

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

        public bool VC_IsDrinking => !(VC_Analysis is null) && VC_Analysis.IsDrinking[VC_StepCounter];

        public bool VC_IsNoseCasting => !(VC_Analysis is null) && VC_Analysis.IsNoseCasting[VC_StepCounter];

        public bool VC_IsSniffing => !(VC_Analysis is null) && VC_Analysis.IsSniffing[VC_StepCounter];

        public bool VC_IsVideoLoaded => model.IsVideoLoaded;

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

        public int VC_StepCounter
        {
            get => stepCounter;
            set
            {
                stepCounter = value;
                VC_FramePath = VC_Analysis.Path[VC_StepCounter].Replace("@WORKING_PATH", model.ArchivePath);

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

        public int VC_TimeStep => (VC_Analysis is null) ? 0 : VC_Analysis.TimeStep[VC_StepCounter];

        public float VC_VelocityX => (VC_Analysis is null) ? 0 : VC_Analysis.VelocityX[VC_StepCounter];

        public float VC_VelocityY => (VC_Analysis is null) ? 0 : VC_Analysis.VelocityY[VC_StepCounter];
        public float VC_X => (VC_Analysis is null) ? 0 : VC_Analysis.X[VC_StepCounter];

        public float VC_Y => (VC_Analysis is null) ? 0 : VC_Analysis.Y[VC_StepCounter];

        public Dictionary<Tuple<int, int>, List<string>> VC_FeaturesTimeRanges => VC_Analysis?.GetFeaturesTimes();
        public Dictionary<Tuple<int, int>, Color> VC_ColorRanges => (VC_Analysis is null) ? null : GetColorsRanges(VC_Analysis);

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

        public Dictionary<Tuple<int, int>, Color> GetColorsRanges(Analysis analysis)
        {
            List<List<string>> fsubs = analysis.GetFeaturesSubsets();
            Dictionary<Tuple<int, int>, List<string>> ftimes = analysis.GetFeaturesTimes();
            Dictionary<Tuple<int, int>, Color> fcolors = new Dictionary<Tuple<int, int>, Color>();
            // TODO: treat a case with more features
            List<Color> colorsList = new List<Color>
            {
                Colors.IndianRed,
                Colors.Navy,
                Colors.Orchid,
                Colors.SaddleBrown,
                Colors.DarkSalmon,
                Colors.MediumSeaGreen
            };
            Dictionary<List<string>, Color> featuresToColors = new Dictionary<List<string>, Color>();
            for (int i = 0; i < fsubs.Count; i++)
                featuresToColors[fsubs[i]] = colorsList[i];
            foreach (KeyValuePair<Tuple<int, int>, List<string>> entry in ftimes)
            {
                Console.WriteLine(featuresToColors.ContainsKey(entry.Value));
                // TODO: switch keys and values or something like that, since it's not the same list thus it doesn't work
                fcolors[entry.Key] = featuresToColors[entry.Value];
            }
            return fcolors;
        }

        public void InitializeVideo(string videoID)
        {
            Video video = model.DBHandler.GetVideoByID(videoID);
            VC_Analysis = model.DBHandler.GetAnalysisByID(video.Analysis);
            //GetColorsRanges();
            //Dictionary<Tuple<int, int>, List<string>> fTimes = model.DBHandler.GetFeaturesTimes(VC_Analysis);
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