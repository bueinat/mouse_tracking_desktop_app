using mouse_tracking_web_app.DataBase;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.Models
{
    // TODO: write code for size changing

    public class DataRows
    {
        public IEnumerable<DataRow> AnalysisDataRows;
        public List<double> Time { get; set; }
        public List<double> X { get; set; }
        public List<double> Y { get; set; }
        public List<double> V { get; set; }
        public List<double> A { get; set; }
        private readonly List<float> vxList;
        private readonly List<float> vyList;
        private readonly List<float> axList;
        private readonly List<float> ayList;

        private double Norm(double x, double min, double max, double listMin, double listMax)
        {
            return (x - listMin) / (listMax - listMin) * (max - min) + min;
        }

        public List<double> NormList(List<double> list, double min, double max)
        {
            if (min == max)
                return list;
            return  list.Select(t => Norm(t, min, max, list.Min(), list.Max())).ToList();
        }

        public DataRows(Analysis analysis)
        {
            AnalysisDataRows = analysis.AnalysisDataTable.Rows.OfType<DataRow>();
            X = AnalysisDataRows?.Select(dr => (double)dr.Field<float>("X")).ToList();
            Y = AnalysisDataRows?.Select(dr => (double)dr.Field<float>("Y")).ToList();
            Time = AnalysisDataRows?.Select(dr => (double)dr.Field<int>("TimeStep")).ToList();

            vxList = AnalysisDataRows?.Select(dr => dr.Field<float>("VelocityX")).ToList();
            vyList = AnalysisDataRows?.Select(dr => dr.Field<float>("VelocityY")).ToList();
            V = new List<double>();
            foreach ((float vxi, float vyi) in vxList.Zip(vyList, (x, y) => (vxi: x, vyi: y)))
                V.Add(Math.Sqrt(vxi * vxi + vyi * vyi));

            axList = AnalysisDataRows?.Select(dr => dr.Field<float>("AccelerationX")).ToList();
            ayList = AnalysisDataRows?.Select(dr => dr.Field<float>("AccelerationY")).ToList();
            A = new List<double>();
            foreach ((float axi, float ayi) in axList.Zip(ayList, (x, y) => (axi: x, ayi: y)))
                A.Add(Math.Sqrt(axi * axi + ayi * ayi));
        }
    }

    public class PlottingControllerModel : INotifyPropertyChanged
    {
        private readonly MainControllerModel model;

        private string colorParam;
        private string sizeParam;

        private bool isLoading = false;
        private ScatterSeries pathPoints;

        private PlotModel plotModel;

        private double minSize;

        public DataRows PC_AnalysisDataRows
        {
            get => model.AnalysisDataRows;
            set
            {
                model.AnalysisDataRows = value;
                NotifyPropertyChanged("PC_AnalysisDataRows");
            }
        }

        public double PC_MinSize
        {
            get => minSize;
            set
            {
                minSize = value;
                NotifyPropertyChanged("PC_MinSize");
            }
        }

        private double maxSize;

        public double PC_MaxSize
        {
            get => maxSize;
            set
            {
                maxSize = value;
                NotifyPropertyChanged("PC_MaxSize");
            }
        }

        public PlottingControllerModel(MainControllerModel model)
        {
            this.model = model;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("PC_" + e.PropertyName);
                if (e.PropertyName == "VideoAnalysis")
                    UpdateModel();
            };
            PC_PlotModel = new PlotModel();
            SetUpModel();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public LinearColorAxis ColoredAx => (ColorList.Count == 0)
                    ? null
                    : new LinearColorAxis
                    {
                        Position = AxisPosition.Right,
                        Minimum = ColorList.Min(),
                        Maximum = ColorList.Max(),
                        Palette = OxyPalettes.Viridis(),
                        InvalidNumberColor = OxyColors.Gray
                    };

        public List<double> ColorList => GetScatterList(PC_ColorParameter, false);
        public List<double> SizeList => GetScatterList(PC_SizeParameter, true);

        //public IEnumerable<DataRow> PC_AnalysisDataRows => PC_VideoAnalysis?.AnalysisDataTable?.Rows.OfType<DataRow>();

        public string PC_ColorParameter
        {
            get => colorParam;
            set
            {
                colorParam = value;
                UpdateModel();
                NotifyPropertyChanged("PC_ColorParameter");
            }
        }

        public string PC_SizeParameter
        {
            get => sizeParam;
            set
            {
                sizeParam = value;
                UpdateModel();
                NotifyPropertyChanged("PC_SizeParameter");
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
        //public List<float> X => PC_AnalysisDataRows?.Select(dr => dr.Field<float>("X")).ToList();
        //public List<float> Y => PC_AnalysisDataRows?.Select(dr => dr.Field<float>("Y")).ToList();

        private double Norm(double x, double min, double max, double listMin, double listMax)
        {
            return (x - listMin) / (listMax - listMin) * (max - min) + min;
        }

        //private List<double> timeList;
        //private List<float> xList;
        //private List<float> yList;
        //private List<float> vxList;
        //private List<float> vyList;
        //private List<double> vList;
        //private List<float> axList;
        //private List<float> ayList;
        //private List<double> aList;

        //public void UpdateLists()
        //{
        //    timeList = PC_AnalysisDataRows?.Select(dr => (double)dr.Field<int>("TimeStep")).ToList();

        //    vxList = PC_AnalysisDataRows?.Select(dr => dr.Field<float>("VelocityX")).ToList();
        //    vyList = PC_AnalysisDataRows?.Select(dr => dr.Field<float>("VelocityY")).ToList();
        //    vList = new List<double>();
        //    foreach ((float vxi, float vyi) in vxList.Zip(vyList, (x, y) => (vxi: x, vyi: y)))
        //        vList.Add(Math.Sqrt(vxi * vxi + vyi * vyi));

        //    axList = PC_AnalysisDataRows?.Select(dr => dr.Field<float>("AccelerationX")).ToList();
        //    ayList = PC_AnalysisDataRows?.Select(dr => dr.Field<float>("AccelerationY")).ToList();
        //    aList = new List<double>();
        //    foreach ((float axi, float ayi) in axList.Zip(ayList, (x, y) => (axi: x, ayi: y)))
        //        aList.Add(Math.Sqrt(axi * axi + ayi * ayi));
        //}

        public Analysis PC_VideoAnalysis
        {
            get => model.VideoAnalysis;
            set
            {
                model.VideoAnalysis = value;
                //PC_AnalysisDataRows = new DataRows(PC_VideoAnalysis);
                NotifyPropertyChanged("PC_VideoAnalysis");
                NotifyPropertyChanged("PC_AnalysisDataTable");
                NotifyPropertyChanged("PC_AnalysisDataRows");
                //UpdateLists();
                UpdateModel();
            }
        }


        // TODO:
        // * shorten its time
        // * create one box for range and then parse it to tuple
        // * MOST IMPORTANT: create a video for Rafi and edit it.
        // * add an option of hiding inactive features
        // * generalizing features
        public List<double> GetScatterList(string parameter, bool normalize)
        {
            double min = double.IsNaN(PC_MinSize) ? 2 : PC_MinSize;
            double max = double.IsNaN(PC_MaxSize) ? 2 : PC_MaxSize;
            if (parameter == "timestep")
                return normalize ? PC_AnalysisDataRows.NormList(PC_AnalysisDataRows.Time, min, max) : PC_AnalysisDataRows.Time;
            if (parameter == "velocity")
                return normalize ? PC_AnalysisDataRows.NormList(PC_AnalysisDataRows.V, min, max) : PC_AnalysisDataRows.V;
            //return normalize ? vList.Select(v => Norm(v, min, max, vList.Min(), vList.Max())).ToList() : vList;
            if (parameter == "acceleration")
                return normalize ? PC_AnalysisDataRows.NormList(PC_AnalysisDataRows.A, min, max) : PC_AnalysisDataRows.A;
            //return normalize ? aList.Select(a => Norm(a, min, max, aList.Min(), aList.Max())).ToList() : aList;
            return new List<double>();
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //if (propertyName == "PC_AnalysisDataRows")
            //    UpdateLists();
        }

        public void UpdateModel() => new Task(UpdateModelWrapped).Start();

        public void UpdateModelWrapped()
        {
            if (PC_AnalysisDataRows is null) return;
                PC_IsLoading = true;
            pathPoints = new ScatterSeries()
            {
                MarkerType = MarkerType.Circle,
                //MarkerSize = 2
            };
            
            if (ColorList.Count > 0)
            {
                PC_PlotModel.Axes.Clear();
                SetUpModel();
                if (SizeList.Count > 0)
                {
                    for (int i = 0; i < PC_AnalysisDataRows.X.Count; i++)
                        pathPoints.Points.Add(new ScatterPoint(PC_AnalysisDataRows.X[i], PC_AnalysisDataRows.Y[i]) { Value = ColorList[i], Size = SizeList[i] });
                }
                else
                {
                    for (int i = 0; i < PC_AnalysisDataRows.X.Count; i++)
                        pathPoints.Points.Add(new ScatterPoint(PC_AnalysisDataRows.X[i], PC_AnalysisDataRows.Y[i]) { Value = ColorList[i], Size = 2 });
                }
            }
            else if (SizeList.Count > 0)
            {
                for (int i = 0; i < PC_AnalysisDataRows.X.Count; i++)
                {
                    pathPoints.MarkerFill = OxyColors.IndianRed;
                    pathPoints.Points.Add(new ScatterPoint(PC_AnalysisDataRows.X[i], PC_AnalysisDataRows.Y[i]) { Size = SizeList[i] });
                }
            }
            else
            {
                for (int i = 0; i < PC_AnalysisDataRows.X.Count; i++)
                {
                    pathPoints.MarkerFill = OxyColors.IndianRed;
                    pathPoints.Points.Add(new ScatterPoint(PC_AnalysisDataRows.X[i], PC_AnalysisDataRows.Y[i]) { Size = 2 });
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
            });

            PC_PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                IsZoomEnabled = false,
            });

            if (!(ColoredAx is null))
                PC_PlotModel.Axes.Add(ColoredAx);
        }
    }
}