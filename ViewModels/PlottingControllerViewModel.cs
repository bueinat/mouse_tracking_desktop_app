using mouse_tracking_web_app.DataBase;
using mouse_tracking_web_app.Models;
using OxyPlot;
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
        public DataTable VMPC_AnalysisDataTable => Model.PC_AnalysisDataTable;

        public string VMPC_ColorParameter
        {
            get => Model.PC_ColorParameter;
            set => Model.PC_ColorParameter = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PlottingControllerModel Model { get; }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}