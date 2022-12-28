using mouse_tracking_web_app.DataBase;
using mouse_tracking_web_app.Models;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.ViewModels
{
    public class PlottingControllerViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        public PlottingControllerViewModel(PlottingControllerModel model)
        {
            Model = model;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("VM" + e.PropertyName);
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Error => null;

        public PlottingControllerModel Model { get; }

        public DataRows VMPC_AnalysisDataTable => Model.PC_AnalysisDataRows;

        public double VMPC_AverageAcceleration => Model.PC_AverageAcceleration;

        public double VMPC_AverageSpeed => Model.PC_AverageSpeed;

        public string VMPC_ColorParameter
        {
            get => Model.PC_ColorParameter;
            set => Model.PC_ColorParameter = value;
        }

        public Dictionary<string, double> VMPC_FeaturesPercents => Model.PC_FeaturesPercents;

        public bool VMPC_IsLoading
        {
            get => Model.PC_IsLoading;
            set => Model.PC_IsLoading = value;
        }

        public double VMPC_MaxSize => Model.PC_MaxSize;

        public double VMPC_MinSize => Model.PC_MinSize;

        public int VMPC_NSteps => Model.PC_NSteps;

        public PlotController VMPC_PlotController => Model.PC_PlotController;

        public PlotModel VMPC_PlotModel => Model.PC_PlotModel;

        public string VMPC_SizeParameter
        {
            get => Model.PC_SizeParameter;
            set => Model.PC_SizeParameter = value;
        }

        public Tuple<double, double> VMPC_SizeRange
        {
            get => Model.PC_SizeRange;
            set => Model.PC_SizeRange = value;
        }

        public string VMPC_StringSizeRange
        {
            get => Model.PC_StringSizeRange;
            set => Model.PC_StringSizeRange = value;
        }

        public double VMPC_TotalDistance => Model.PC_TotalDistance;

        public Analysis VMPC_VideoAnalysis => Model.PC_VideoAnalysis;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "VMPC_StringSizeRange":
                        string pattern = @"^\s*\d+\.?\d*\s*$";
                        if (VMPC_SizeRange is null)
                            VMPC_SizeRange = new Tuple<double, double>(double.NaN, double.NaN);
                        if (string.IsNullOrEmpty(VMPC_StringSizeRange))
                            return string.Empty;
                        string[] vSplit = VMPC_StringSizeRange.Split('-');
                        if (vSplit.Length == 1)
                        {
                            MatchCollection matches = Regex.Matches(vSplit[0], pattern);
                            if (matches.Count == 0)
                                return $"{vSplit[0]} is not in\nfloat format.";
                            VMPC_SizeRange = new Tuple<double, double>(double.Parse(vSplit[0]), double.Parse(vSplit[0]));
                        }
                        else if (vSplit.Length == 2)
                        {
                            MatchCollection match1 = Regex.Matches(vSplit[0], pattern);
                            MatchCollection match2 = Regex.Matches(vSplit[1], pattern);
                            if ((match1.Count == 0) || (match2.Count == 0))
                                return $"{VMPC_StringSizeRange} is not in\nfloat range format.";
                            if (double.Parse(vSplit[0]) >= double.Parse(vSplit[1]))
                                return $"not a valid range,\nsince {vSplit[1]}  >= {vSplit[0]}";
                            VMPC_SizeRange = new Tuple<double, double>(double.Parse(vSplit[0]), double.Parse(vSplit[1]));
                        }
                        else
                            return "Illegal characters";
                        return string.Empty;

                    default:
                        break;
                }

                return string.Empty;
            }
        }

        public void ApplyChanges()
        {
            new Task(Model.UpdateModel).Start();
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}