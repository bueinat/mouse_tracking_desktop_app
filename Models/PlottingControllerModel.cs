using mouse_tracking_web_app.DataBase;
using mouse_tracking_web_app.ViewModels;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.Models
{
    public class PlottingControllerModel : INotifyPropertyChanged
    {
        public List<double> PC_ColorList;

        public SettingsManager SM;

        private readonly MainControllerModel model;

        private readonly List<MarkerType> mTypes = new List<MarkerType>
        {
            MarkerType.Circle,
            MarkerType.Square,
            MarkerType.Diamond,
            MarkerType.Triangle,
            MarkerType.Cross,
            MarkerType.Plus,
            MarkerType.Star
        };

        private string colorParam;
        private bool isLoading = false;

        private PlotModel plotModel;
        private Dictionary<string, MarkerType> scatterTypes;
        private string sizeParam;
        private Tuple<double, double> sizeRange = new Tuple<double, double>(double.NaN, double.NaN);
        private string stringSizeRange;

        public PlottingControllerModel(MainControllerModel model, SettingsManager sManager)
        {
            // track changes in model
            this.model = model;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("PC_" + e.PropertyName);
            };

            // track changes in settings
            SM = sManager;
            SM.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("PC_" + e.PropertyName);
                if (e.PropertyName == "PlotMarkerSize" && double.IsNaN(PC_SizeRange.Item1))
                {
                    new Task(UpdateModel).Start();
                }
            };

            int i = 0;
            scatterTypes = new Dictionary<string, MarkerType>
            {
                ["none"] = mTypes[i++]
            };
            foreach (string feature in SM.FeaturesList)
            {
                if (i < mTypes.Count)
                    scatterTypes.Add(feature, mTypes[i++]);
            }

            // create plot controller
            PC_PlotController = new PlotController();

            PC_PlotController.BindMouseDown(OxyMouseButton.Left, PlotCommands.ZoomRectangle);
            PC_PlotController.BindMouseEnter(PlotCommands.HoverSnapTrack);

            // create plot model and set it up
            PC_PlotModel = new PlotModel
            {
                PlotType = PlotType.Cartesian
            };
            SetUpModel();
            PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName.Equals("PC_VideoAnalysis"))
                    new Task(UpdateModel).Start();
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public LinearColorAxis ColoredAx => IsNullOrEmpty(PC_ColorList)
                    ? null
                    : new LinearColorAxis
                    {
                        Position = AxisPosition.Top,
                        Minimum = PC_ColorList.Min(),
                        Maximum = PC_ColorList.Max(),
                        Palette = OxyPalettes.Viridis(),
                        InvalidNumberColor = OxyColors.Gray,
                        Title = PC_ColorParameter
                    };

        public double DefaultMarkerSize => SM is null ? double.NaN : SM.PlotMarkerSize;
        public DataRows PC_AnalysisDataRows => model.AnalysisDataRows;

        public double PC_AverageAcceleration => model.AverageAcceleration;

        public double PC_AverageSpeed => model.AverageSpeed;

        public string PC_ColorParameter
        {
            get => colorParam;
            set
            {
                colorParam = value;
                NotifyPropertyChanged("PC_ColorParameter");
            }
        }

        public Dictionary<string, double> PC_FeaturesPercents => model.FeaturesPercents;

        public bool PC_IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                NotifyPropertyChanged("PC_IsLoading");
            }
        }

        public double PC_MaxSize => double.IsNaN(PC_SizeRange.Item2) ? DefaultMarkerSize : PC_SizeRange.Item2;

        public double PC_MinSize => double.IsNaN(PC_SizeRange.Item1) ? DefaultMarkerSize : PC_SizeRange.Item1;

        public int PC_NSteps => model.NSteps;
        public PlotController PC_PlotController { get; private set; }

        public PlotModel PC_PlotModel
        {
            get => plotModel;
            set
            {
                plotModel = value;
                NotifyPropertyChanged("PC_PlotModel");
            }
        }

        public List<double> PC_SizeList { get; set; }

        public string PC_SizeParameter
        {
            get => sizeParam;
            set
            {
                sizeParam = value;
                NotifyPropertyChanged("PC_SizeParameter");
            }
        }

        public Tuple<double, double> PC_SizeRange
        {
            get => sizeRange;
            set
            {
                sizeRange = value;
                NotifyPropertyChanged("PC_SizeRange");
                NotifyPropertyChanged("PC_MaxSize");
                NotifyPropertyChanged("PC_MinSize");
                NotifyPropertyChanged("PC_SizeParameter");
            }
        }

        public string PC_StringSizeRange
        {
            get => stringSizeRange;
            set
            {
                stringSizeRange = value;
                if (string.IsNullOrEmpty(value))
                    PC_SizeRange = new Tuple<double, double>(double.NaN, double.NaN);
                else
                {
                    string[] vSplit = value.Split('-');
                    if (vSplit.Length == 1)
                        PC_SizeRange = new Tuple<double, double>(double.Parse(vSplit[0]), double.Parse(vSplit[0]));
                    else if (vSplit.Length == 2)
                        PC_SizeRange = new Tuple<double, double>(double.Parse(vSplit[0]), double.Parse(vSplit[1]));
                }
            }
        }

        public double PC_TotalDistance => model.TotalDistance;

        public Analysis PC_VideoAnalysis => model.VideoAnalysis;

        public AnalysisStats PC_VideoStats => model.VideoStats;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateModel()
        {
            if (PC_AnalysisDataRows is null) return;

            PC_IsLoading = true;
            Dictionary<string, ScatterSeries> pathPoints = CreateEmptyPathPoints();
            UpdateScatterList("PC_ColorList", PC_ColorParameter, false);
            UpdateScatterList("PC_SizeList", PC_SizeParameter, true);

            for (int i = 0; i < PC_AnalysisDataRows.X.Count; i++)
            {
                bool wasAdded = false;
                foreach (KeyValuePair<string, bool> item in PC_VideoAnalysis.Features[i])
                {
                    if (item.Value)
                    {
                        pathPoints[item.Key].Points.Add(new ScatterPoint(PC_AnalysisDataRows.X[i], 480 - PC_AnalysisDataRows.Y[i]) { });
                        if (!IsNullOrEmpty(PC_ColorList))
                            pathPoints[item.Key].Points[pathPoints[item.Key].Points.Count - 1].Value = PC_ColorList[i];

                        if (!IsNullOrEmpty(PC_SizeList))
                            pathPoints[item.Key].Points[pathPoints[item.Key].Points.Count - 1].Size = PC_SizeList[i];

                        wasAdded = true;
                    }
                }
                if (!wasAdded)
                {
                    pathPoints["none"].Points.Add(new ScatterPoint(PC_AnalysisDataRows.X[i], 480 - PC_AnalysisDataRows.Y[i]) { });
                    if (!IsNullOrEmpty(PC_ColorList))
                        pathPoints["none"].Points[pathPoints["none"].Points.Count - 1].Value = PC_ColorList[i];

                    if (!IsNullOrEmpty(PC_SizeList))
                        pathPoints["none"].Points[pathPoints["none"].Points.Count - 1].Size = PC_SizeList[i];
                }
            }

            if (!IsNullOrEmpty(PC_ColorList))
            {
                PC_PlotModel.Axes.Clear();
                SetUpModel();
                foreach (KeyValuePair<string, ScatterSeries> keyVal in pathPoints)
                {
                    keyVal.Value.TrackerFormatString += "\n" + PC_ColorParameter + " = {Value:0.##}";
                }
            }
            else
            {
                PC_PlotModel.Axes.Clear();
                SetUpModel();
                foreach (KeyValuePair<string, ScatterSeries> keyVal in pathPoints)
                    keyVal.Value.MarkerFill = OxyColors.MediumSeaGreen; // ******* //
            }

            if (!IsNullOrEmpty(PC_SizeList) && (PC_MaxSize > PC_MinSize))
            {
                foreach (KeyValuePair<string, ScatterSeries> keyVal in pathPoints)
                {
                    keyVal.Value.TrackerFormatString += "\nsize = {Size:0.##}";
                }
            }
            else
            {
                foreach (KeyValuePair<string, ScatterSeries> keyVal in pathPoints)
                {
                    for (int i = 0; i < keyVal.Value.Points.Count; i++)
                        keyVal.Value.Points[i].Size = PC_MinSize;
                }
            }

            PC_PlotModel.Series.Clear();
            foreach (KeyValuePair<string, ScatterSeries> keyVal in pathPoints)
            {
                if (!keyVal.Key.Equals("none"))
                    keyVal.Value.TrackerFormatString += "\nfeature = " + keyVal.Key;
                PC_PlotModel.Series.Add(keyVal.Value);
            }

            PC_PlotModel.InvalidatePlot(true);
            PC_IsLoading = false;
        }

        public void UpdateScatterList(string propertyName, string parameter, bool normalize)
        {
            List<double> list = new List<double>();
            if (PC_AnalysisDataRows is null) list = null;
            else if (propertyName == "PC_ColorList" && PC_ColorParameter == "none") list = null;
            else if (parameter == "timestep")
                list = normalize ? PC_AnalysisDataRows.NormList(PC_AnalysisDataRows.Time, PC_MinSize, PC_MaxSize) : PC_AnalysisDataRows.Time;
            else if (parameter == "velocity")
                list = normalize ? PC_AnalysisDataRows.NormList(PC_AnalysisDataRows.V, PC_MinSize, PC_MaxSize) : PC_AnalysisDataRows.V;
            else if (parameter == "acceleration")
                list = normalize ? PC_AnalysisDataRows.NormList(PC_AnalysisDataRows.A, PC_MinSize, PC_MaxSize) : PC_AnalysisDataRows.A;
            if (propertyName == "PC_SizeList")
                PC_SizeList = list;
            if (propertyName == "PC_ColorList")
                PC_ColorList = list;
        }

        private Dictionary<string, ScatterSeries> CreateEmptyPathPoints()
        {
            Dictionary<string, ScatterSeries> pathPoints = new Dictionary<string, ScatterSeries>();
            foreach (KeyValuePair<string, MarkerType> keyVal in scatterTypes)
            {
                pathPoints[keyVal.Key] = new ScatterSeries()
                {
                    MarkerType = keyVal.Value,
                    MarkerStroke = OxyColors.White,
                    MarkerStrokeThickness = 0,
                    TrackerFormatString = "position = ({X:0.##}, {Y:0.##})"
                };
            }

            return pathPoints;
        }

        private bool IsNullOrEmpty<T>(List<T> list)
        {
            return (list is null) || (list.Count == 0);
        }

        private void SetUpModel()
        {
            // TODO: get arena size from python script
            PC_PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 752,
                Title = "X"
            });

            PC_PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 480,
                Title = "Y"
            });

            if (!(ColoredAx is null))
                PC_PlotModel.Axes.Add(ColoredAx);
        }
    }
}