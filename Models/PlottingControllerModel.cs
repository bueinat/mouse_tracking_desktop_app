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
    public class PlottingControllerModel : INotifyPropertyChanged
    {
        private readonly MainControllerModel model;

        private string colorParam;

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

        private bool isLoading = false;

        public bool PC_IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                NotifyPropertyChanged("PC_IsLoading");
            }
        }

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

        public List<double> ColorList => GetScatterColorList();
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

        public List<double> GetScatterColorList()
        {
            List<double> points = new List<double>();
            if (PC_ColorParameter == "timestep")
                return PC_AnalysisDataTable.Rows.OfType<DataRow>()
                        .Select(dr => (double)dr.Field<int>("TimeStep")).ToList();
            if (PC_ColorParameter == "velocity")
            {
                List<float> vx = PC_AnalysisDataTable.Rows.OfType<DataRow>()
                            .Select(dr => dr.Field<float>("VelocityX")).ToList();
                List<float> vy = PC_AnalysisDataTable.Rows.OfType<DataRow>()
                            .Select(dr => dr.Field<float>("VelocityY")).ToList();

                foreach ((float vxi, float vyi) in vx.Zip(vy, (x, y) => (vxi: x, vyi: y)))
                    points.Add(Math.Sqrt(vxi * vxi + vyi * vyi));
                return points;
            }
            if (PC_ColorParameter == "acceleration")
            {
                List<float> ax = PC_AnalysisDataTable.Rows.OfType<DataRow>()
                            .Select(dr => dr.Field<float>("VelocityX")).ToList();
                List<float> ay = PC_AnalysisDataTable.Rows.OfType<DataRow>()
                            .Select(dr => dr.Field<float>("VelocityY")).ToList();

                foreach ((float axi, float ayi) in ax.Zip(ay, (x, y) => (axi: x, ayi: y)))
                    points.Add(Math.Sqrt(axi * axi + ayi * ayi));
                return points;
            }
            return points;
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateModelWrapped()
        {
            PC_IsLoading = true;
            pathPoints = new ScatterSeries()
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 2
            };

            if (ColorList.Count > 0)
            {
                PC_PlotModel.Axes.Clear();
                SetUpModel();

                for (int i = 0; i < X.Count; i++)
                    pathPoints.Points.Add(new ScatterPoint(X[i], Y[i]) { Value = ColorList[i] });
            }
            else
            {
                for (int i = 0; i < X.Count; i++)
                {
                    pathPoints.MarkerFill = OxyColors.IndianRed;
                    pathPoints.Points.Add(new ScatterPoint(X[i], Y[i]) { });
                }
            }

            PC_PlotModel.Series.Clear();
            PC_PlotModel.Series.Add(pathPoints);

            PC_PlotModel.InvalidatePlot(true);
            PC_IsLoading = false;
        }

        public void UpdateModel()
        {
            new Task(UpdateModelWrapped).Start();
        }

        private void SetUpModel()
        {
            // TODO: get arena size from python script
            PC_PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                IsZoomEnabled = false,
                Minimum = 0,
            });

            PC_PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                IsZoomEnabled = false,
                Minimum = 0,
            });

            //PC_PlotModel.PlotType = PlotType.Cartesian;

            if (!(ColoredAx is null))
                PC_PlotModel.Axes.Add(ColoredAx);
        }
    }
}