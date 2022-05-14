using mouse_tracking_web_app.DataBase;
using mouse_tracking_web_app.Models;
using OxyPlot;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace mouse_tracking_web_app.ViewModels
{
    public class PlottingControllerViewModel : INotifyPropertyChanged
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

        public PlotModel VMPC_PlotModel => Model.PC_PlotModel;
        public Analysis VMPC_VideoAnalysis => Model.PC_VideoAnalysis;
        public DataRows VMPC_AnalysisDataTable => Model.PC_AnalysisDataRows;

        public string VMPC_ColorParameter
        {
            get => Model.PC_ColorParameter;
            set => Model.PC_ColorParameter = value;
        }

        public string VMPC_SizeParameter
        {
            get => Model.PC_SizeParameter;
            set => Model.PC_SizeParameter = value;
        }

        public double VMPC_MinSize
        {
            get => Model.PC_MinSize;
            set => Model.PC_MinSize = value;
        }

        public double VMPC_MaxSize
        {
            get => Model.PC_MaxSize;
            set => Model.PC_MaxSize = value;
        }

        public bool VMPC_IsLoading
        {
            get => Model.PC_IsLoading;
            set => Model.PC_IsLoading = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PlottingControllerModel Model { get; }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}