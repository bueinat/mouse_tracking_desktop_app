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
        private readonly MainControllerModel model;

        private string colorParam;
        private bool isLoading = false;
        private ScatterSeries pathPoints;
        private PlotModel plotModel;
        private string sizeParam;
        private Tuple<double, double> sizeRange = new Tuple<double, double>(double.NaN, double.NaN);
        private readonly double defaultMarkerSize = double.Parse(ConfigurationManager.AppSettings.Get("PlotDefaultMarkerSize"));

        private readonly List<string> propNames = new List<string>
            {
                "PC_VideoAnalysis",
                "PC_MinSize",
                "PC_MaxSize",
                "PC_ColorParameter",
                "PC_SizeParameter",
            };

        public PlotController PC_PlotController { get; private set; }

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
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public LinearColorAxis ColoredAx => (PC_ColorList.Count == 0)
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

        public List<double> PC_ColorList; // => GetScatterList(PC_ColorParameter, false);

        // TODO:
        // * shorten processing time
        // * MOST IMPORTANT: create a video for Rafi and edit it.
        // * add an option of hiding inactive features
        // * generalizing features
        // * improve range thing
        public double PC_MaxSize => double.IsNaN(PC_SizeRange.Item2) ? defaultMarkerSize : PC_SizeRange.Item2;

        public double PC_MinSize => double.IsNaN(PC_SizeRange.Item1) ? defaultMarkerSize : PC_SizeRange.Item1;

        public DataRows PC_AnalysisDataRows
        {
            get => model.AnalysisDataRows;
            set
            {
                model.AnalysisDataRows = value;
                NotifyPropertyChanged("PC_AnalysisDataRows");
            }
        }

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

        public bool PC_IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                NotifyPropertyChanged("PC_IsLoading");
            }
        }

        public PlotModel PC_PlotModel
        {
            get => plotModel;
            set
            {
                plotModel = value;
                NotifyPropertyChanged("PC_PlotModel");
            }
        }

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

        public Analysis PC_VideoAnalysis
        {
            get => model.VideoAnalysis;
            set
            {
                model.VideoAnalysis = value;
                NotifyPropertyChanged("PC_VideoAnalysis");
                NotifyPropertyChanged("PC_AnalysisDataTable");
                NotifyPropertyChanged("PC_AnalysisDataRows");
            }
        }

        public List<double> PC_SizeList { get; set; } // => GetScatterList(PC_SizeParameter, true);

        public List<double> GetScatterList(string parameter, bool normalize)
        {
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

        //public void UpdateModel() => new Task(UpdateModelWrapped).Start();

        public void UpdateModel()
        {
            if (PC_AnalysisDataRows is null) return;
            PC_IsLoading = true;
            pathPoints = new ScatterSeries()
            {
                MarkerType = MarkerType.Circle,
                TrackerFormatString = "position = ({X:0.##}, {Y:0.##})"
            };
            if (PC_ColorList.Count > 0)
            {
                PC_PlotModel.Axes.Clear();
                pathPoints.TrackerFormatString += "\n" + PC_ColorParameter + " = {Value:0.##}";

                SetUpModel();
                if ((PC_SizeList.Count > 0) && (PC_MaxSize > PC_MinSize))
                {
                    pathPoints.TrackerFormatString += "\nsize = {Size:0.##}";
                    for (int i = 0; i < PC_AnalysisDataRows.X.Count; i++)
                        pathPoints.Points.Add(new ScatterPoint(PC_AnalysisDataRows.X[i], PC_AnalysisDataRows.Y[i]) { Value = PC_ColorList[i], Size = PC_SizeList[i] });
                }
                else
                {
                    for (int i = 0; i < PC_AnalysisDataRows.X.Count; i++)
                        pathPoints.Points.Add(new ScatterPoint(PC_AnalysisDataRows.X[i], PC_AnalysisDataRows.Y[i]) { Value = PC_ColorList[i], Size = PC_MinSize });
                }
            }
            else if ((PC_SizeList.Count > 0) && (PC_MaxSize > PC_MinSize))
            {
                pathPoints.TrackerFormatString += "\nsize = {Size:0.##}";
                for (int i = 0; i < PC_AnalysisDataRows.X.Count; i++)
                {
                    pathPoints.MarkerFill = OxyColors.IndianRed;
                    pathPoints.Points.Add(new ScatterPoint(PC_AnalysisDataRows.X[i], PC_AnalysisDataRows.Y[i]) { Size = PC_SizeList[i] });
                }
            }
            else
            {
                for (int i = 0; i < PC_AnalysisDataRows.X.Count; i++)
                {
                    pathPoints.MarkerFill = OxyColors.IndianRed;
                    pathPoints.Points.Add(new ScatterPoint(PC_AnalysisDataRows.X[i], PC_AnalysisDataRows.Y[i]) { Size = PC_MinSize });
                }
            }

            PC_PlotModel.Series.Clear();
            PC_PlotModel.Series.Add(pathPoints);

            PC_PlotModel.InvalidatePlot(true);
            PC_IsLoading = false;
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