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

    public class PlottingControllerModel : INotifyPropertyChanged
    {
        private readonly MainControllerModel model;

        private string colorParam;
        private string sizeParam;

        private bool isLoading = false;
        private ScatterSeries pathPoints;

        private PlotModel plotModel;

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

        public DataTable PC_AnalysisDataTable => PC_VideoAnalysis?.AnalysisDataTable;

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

        public Analysis PC_VideoAnalysis
        {
            get => model.VideoAnalysis;
            set
            {
                model.VideoAnalysis = value;
                NotifyPropertyChanged("PC_VideoAnalysis");
                NotifyPropertyChanged("PC_AnalysisDataTable");
                UpdateModel();
            }
        }

        public List<float> X => PC_AnalysisDataTable?.Rows.OfType<DataRow>()
                .Select(dr => dr.Field<float>("X")).ToList();

        public List<float> Y => PC_AnalysisDataTable?.Rows.OfType<DataRow>()
                .Select(dr => dr.Field<float>("Y")).ToList();

        public List<double> GetScatterList(string parameter, bool normalize)
        {
            double nfactor = 5;
            List<double> points = new List<double>();
            if (parameter == "timestep")
            {
                List<double> list = PC_AnalysisDataTable.Rows.OfType<DataRow>()
                        .Select(dr => (double)dr.Field<int>("TimeStep")).ToList();
                return normalize
                    ? PC_AnalysisDataTable.Rows.OfType<DataRow>()
                        .Select(dr => nfactor * dr.Field<int>("TimeStep") / list.Max()).ToList()
                    : list;
            }
            if (parameter == "velocity")
            {
                List<float> vx = PC_AnalysisDataTable.Rows.OfType<DataRow>()
                            .Select(dr => dr.Field<float>("VelocityX")).ToList();
                List<float> vy = PC_AnalysisDataTable.Rows.OfType<DataRow>()
                            .Select(dr => dr.Field<float>("VelocityY")).ToList();

                foreach ((float vxi, float vyi) in vx.Zip(vy, (x, y) => (vxi: x, vyi: y)))
                    points.Add(Math.Sqrt(vxi * vxi + vyi * vyi));
                return normalize ? points.Select(v => nfactor * v / points.Max()).ToList() : points;
            }
            if (parameter == "acceleration")
            {
                List<float> ax = PC_AnalysisDataTable.Rows.OfType<DataRow>()
                            .Select(dr => dr.Field<float>("VelocityX")).ToList();
                List<float> ay = PC_AnalysisDataTable.Rows.OfType<DataRow>()
                            .Select(dr => dr.Field<float>("VelocityY")).ToList();

                foreach ((float axi, float ayi) in ax.Zip(ay, (x, y) => (axi: x, ayi: y)))
                    points.Add(Math.Sqrt(axi * axi + ayi * ayi));
                return normalize ? points.Select(a => nfactor * a / points.Max()).ToList() : points;
            }
            return points;
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateModel() => new Task(UpdateModelWrapped).Start();

        public void UpdateModelWrapped()
        {
            PC_IsLoading = true;
            pathPoints = new ScatterSeries()
            {
                MarkerType = MarkerType.Circle,
                //MarkerSize = 2
            };

            if ((ColorList.Count > 0) && (SizeList.Count > 0))
            {
                PC_PlotModel.Axes.Clear();
                SetUpModel();

                for (int i = 0; i < X.Count; i++)
                    pathPoints.Points.Add(new ScatterPoint(X[i], Y[i]) { Value = ColorList[i], Size = SizeList[i] });
            }
            else if (ColorList.Count > 0)
            {
                PC_PlotModel.Axes.Clear();
                SetUpModel();

                for (int i = 0; i < X.Count; i++)
                    pathPoints.Points.Add(new ScatterPoint(X[i], Y[i]) { Value = ColorList[i], Size = 2 });
            }
            else if (SizeList.Count > 0)
            {
                for (int i = 0; i < X.Count; i++)
                {
                    pathPoints.MarkerFill = OxyColors.IndianRed;
                    pathPoints.Points.Add(new ScatterPoint(X[i], Y[i]) { Size = SizeList[i] });
                }
            }
            else
            {
                for (int i = 0; i < X.Count; i++)
                {
                    pathPoints.MarkerFill = OxyColors.IndianRed;
                    pathPoints.Points.Add(new ScatterPoint(X[i], Y[i]) { Size = 2 });
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