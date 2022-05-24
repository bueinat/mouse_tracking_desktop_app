using mouse_tracking_web_app.DataBase;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.Models
{
    public class PlottingControllerModel : INotifyPropertyChanged
    {
        public List<double> PC_ColorList;
        private readonly double defaultMarkerSize = double.Parse(ConfigurationManager.AppSettings.Get("PlotDefaultMarkerSize"));
        private readonly MainControllerModel model;

        private readonly List<string> propNames = new List<string>
            {
                "PC_VideoAnalysis",
                "PC_SizeRange",
                "PC_ColorParameter",
                "PC_SizeParameter",
            };

        private string colorParam;
        private bool isLoading = false;

        //private ScatterSeries pathPoints;
        private Dictionary<string, ScatterSeries> pathPoints = new Dictionary<string, ScatterSeries>();

        private PlotModel plotModel;
        private string sizeParam;
        private Tuple<double, double> sizeRange = new Tuple<double, double>(double.NaN, double.NaN);
        private readonly Dictionary<string, MarkerType> scatterTypes = new Dictionary<string, MarkerType>
        {
            ["none"] = MarkerType.Circle,
            ["sniffing"] = MarkerType.Cross,
            ["drinking"] = MarkerType.Diamond,
            ["noseCasting"] = MarkerType.Square
        };

        private void ResetPathPoints()
        {
            foreach (var keyVal in scatterTypes)
                pathPoints[keyVal.Key] = new ScatterSeries()
                {
                    MarkerType = keyVal.Value,
                    TrackerFormatString = "position = ({X:0.##}, {Y:0.##})"
                };
        }

        public PlottingControllerModel(MainControllerModel model)
        {
            this.model = model;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("PC_" + e.PropertyName);
            };
            PC_PlotController = new PlotController();
            PC_PlotController.UnbindMouseDown(OxyMouseButton.Left);
            PC_PlotController.BindMouseEnter(PlotCommands.HoverSnapTrack);

            PC_PlotModel = new PlotModel();
            SetUpModel();
            PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                if (propNames.Contains(e.PropertyName))
                    new Task(UpdateModel).Start();
            };
            ResetPathPoints();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public LinearColorAxis ColoredAx => IsNullOrEmpty(PC_ColorList)
                    ? null
                    : new LinearColorAxis
                    {
                        Position = AxisPosition.Right,
                        Minimum = PC_ColorList.Min(),
                        Maximum = PC_ColorList.Max(),
                        Palette = OxyPalettes.Viridis(),
                        InvalidNumberColor = OxyColors.Gray,
                        Title = PC_ColorParameter
                    };

        public DataRows PC_AnalysisDataRows => model.AnalysisDataRows;
        public double PC_AverageAcceleration => model.AverageAcceleration;
        public double PC_AverageSpeed => model.AverageSpeed;

        public string PC_ColorParameter
        {
            get => colorParam;
            set
            {
                colorParam = value;
                // TODO: use events for those
                PC_ColorList = GetScatterList(PC_ColorParameter, false);
                NotifyPropertyChanged("PC_ColorParameter");
            }
        }

        public double PC_IsDrinkingPercent => model.IsDrinkingPercent;

        public bool PC_IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                NotifyPropertyChanged("PC_IsLoading");
            }
        }

        public double PC_IsNoseCastingPercent => model.IsNoseCastingPercent;
        public double PC_IsSniffingPercent => model.IsSniffingPercent;

        // TODO:
        // * add an option of hiding inactive features
        // * generalizing features
        public double PC_MaxSize => double.IsNaN(PC_SizeRange.Item2) ? defaultMarkerSize : PC_SizeRange.Item2;

        public double PC_MinSize => double.IsNaN(PC_SizeRange.Item1) ? defaultMarkerSize : PC_SizeRange.Item1;
        public int PC_NSteps => model.NSteps;

        //public int PC_NSteps => PC_VideoStats is null ? 0 : PC_VideoStats.NSteps;
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
                PC_SizeList = GetScatterList(PC_SizeParameter, true);
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
            }
        }

        public string PC_StringSizeRange { get; set; }
        public double PC_TotalDistance => model.TotalDistance;
        public Analysis PC_VideoAnalysis => model.VideoAnalysis;
        public AnalysisStats PC_VideoStats => model.VideoStats;

        public List<double> GetScatterList(string parameter, bool normalize)
        {
            if (PC_AnalysisDataRows is null) return null;
            if (PC_MinSize == PC_MaxSize)
                normalize = false;
            if (parameter == "timestep")
                return normalize ? PC_AnalysisDataRows.NormList(PC_AnalysisDataRows.Time, PC_MinSize, PC_MaxSize) : PC_AnalysisDataRows.Time;
            if (parameter == "velocity")
                return normalize ? PC_AnalysisDataRows.NormList(PC_AnalysisDataRows.V, PC_MinSize, PC_MaxSize) : PC_AnalysisDataRows.V;
            if (parameter == "acceleration")
                return normalize ? PC_AnalysisDataRows.NormList(PC_AnalysisDataRows.A, PC_MinSize, PC_MaxSize) : PC_AnalysisDataRows.A;
            return new List<double>();
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //public DataTable PC_AnalysisDataTable => PC_VideoAnalysis.Get

        //private MarkerType GetMarker(Analysis row)
        //{
        //    return MarkerType.Circle;
        //}

        public void UpdateModel()
        {
            if (PC_AnalysisDataRows is null) return;
            PC_IsLoading = true;
            ResetPathPoints();

            for (int i = 0; i < PC_AnalysisDataRows.X.Count; i++)
            {
                if (PC_VideoAnalysis.IsDrinking[i])
                    pathPoints["drinking"].Points.Add(new ScatterPoint(PC_AnalysisDataRows.X[i], PC_AnalysisDataRows.Y[i]) { Size = PC_MinSize });
                else if (PC_VideoAnalysis.IsSniffing[i])
                    pathPoints["sniffing"].Points.Add(new ScatterPoint(PC_AnalysisDataRows.X[i], PC_AnalysisDataRows.Y[i]) { Size = PC_MinSize });
                else if (PC_VideoAnalysis.IsNoseCasting[i])
                    pathPoints["noseCasting"].Points.Add(new ScatterPoint(PC_AnalysisDataRows.X[i], PC_AnalysisDataRows.Y[i]) { Size = PC_MinSize });
                else
                    pathPoints["none"].Points.Add(new ScatterPoint(PC_AnalysisDataRows.X[i], PC_AnalysisDataRows.Y[i]) { Size = PC_MinSize });
            }

            if (!IsNullOrEmpty(PC_ColorList))
            {
                PC_PlotModel.Axes.Clear();
                SetUpModel();
                foreach (var keyVal in pathPoints)
                {
                    keyVal.Value.TrackerFormatString += "\n" + PC_ColorParameter + " = {Value:0.##}";
                    for (int i = 0; i < PC_AnalysisDataRows.X.Count; i++)
                        keyVal.Value.Points[i].Value = PC_ColorList[i];
                }
            } else
            {
                foreach (var keyVal in pathPoints)
                    keyVal.Value.MarkerFill = OxyColors.IndianRed;
            }

            if (!IsNullOrEmpty(PC_SizeList) && (PC_MaxSize > PC_MinSize))
            {
                foreach (var keyVal in pathPoints)
                {
                    keyVal.Value.TrackerFormatString += "\nsize = {Size:0.##}";
                    for (int i = 0; i < PC_AnalysisDataRows.X.Count; i++)
                        keyVal.Value.Points[i].Size = PC_SizeList[i];
                }
            }

            PC_PlotModel.Series.Clear();
            foreach (var keyVal in pathPoints)
                PC_PlotModel.Series.Add(keyVal.Value);

            PC_PlotModel.InvalidatePlot(true);
            PC_IsLoading = false;
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
                IsZoomEnabled = false,
                Title = "X"
            });

            PC_PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                IsZoomEnabled = false,
                Title = "Y"
            });

            if (!(ColoredAx is null))
                PC_PlotModel.Axes.Add(ColoredAx);
        }
    }
}