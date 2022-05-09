using mouse_tracking_web_app.DataBase;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace mouse_tracking_web_app.Models
{
    public class PlottingControllerModel : INotifyPropertyChanged
    {
        private readonly MainControllerModel model;

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
        }

        private ScatterSeries pathPoints;

        public void UpdateModel()
        {
            pathPoints = new ScatterSeries()
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 2
            };

            List<float> x = PC_AnalysisDataTable.Rows.OfType<DataRow>()
                .Select(dr => dr.Field<float>("x")).ToList();
            List<float> y = PC_AnalysisDataTable.Rows.OfType<DataRow>()
                        .Select(dr => dr.Field<float>("y")).ToList();
            List<int> time = PC_AnalysisDataTable.Rows.OfType<DataRow>()
                        .Select(dr => dr.Field<int>("TimeStep")).ToList();

            PC_PlotModel.Axes.Add(new LinearColorAxis
            {
                Position = AxisPosition.Right,
                Minimum = time.Min(),
                Maximum = time.Max(),
                Palette = OxyPalettes.Viridis(),
                InvalidNumberColor = OxyColors.Gray
        });

            for (int i = 0; i < x.Count; i++)
                pathPoints.Points.Add(new ScatterPoint(x[i], y[i]) { Value = time[i] });

            PC_PlotModel.Series.Clear();
            PC_PlotModel.Series.Add(pathPoints);

            PC_PlotModel.InvalidatePlot(true);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //public PlotModel PC_PlotModel { get; private set; }
        private PlotModel plotModel;

        public PlotModel PC_PlotModel
        {
            get { return plotModel; }
            set
            {
                plotModel = value;
                NotifyPropertyChanged("PC_PlotModel");
                //UpdatePlot();
            }
        }

        public Analysis PC_VideoAnalysis
        {
            get
            {
                //UpdatePlot();
                return model.VideoAnalysis;
            }
            set
            {
                model.VideoAnalysis = value;
                NotifyPropertyChanged("PC_VideoAnalysis");
                NotifyPropertyChanged("PC_AnalysisDataTable");
                NotifyPropertyChanged("PC_Path");
                UpdateModel();
                //var v = PC_Path;
            }
        }

        public DataTable PC_AnalysisDataTable => PC_VideoAnalysis?.AnalysisDataTable;

        private List<DataPoint> GetPathFromAnalysis()
        {
            List<float> x = PC_AnalysisDataTable.Rows.OfType<DataRow>()
                            .Select(dr => dr.Field<float>("x")).ToList();
            List<float> y = PC_AnalysisDataTable.Rows.OfType<DataRow>()
                        .Select(dr => dr.Field<float>("y")).ToList();
            IEnumerable<(float X, float Y)> xy = x.Zip(y, (xi, yi) => (X: xi, Y: yi));
            List<DataPoint> s = new List<DataPoint>();
            foreach ((float X, float Y) in xy)
                s.Add(new DataPoint(X, Y));
            //s.Add({ xyi.X, xyi.Y});
            return s;
        }

        public List<DataPoint> PC_Path
        {
            get
            {
                //PC_PlotModel.Series.Add(GetPathFromAnalysis());
                return GetPathFromAnalysis();
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}