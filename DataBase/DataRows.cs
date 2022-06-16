using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace mouse_tracking_web_app.DataBase
{
    public class DataRows
    {
        public IEnumerable<DataRow> AnalysisDataRows;
        private readonly List<float> axList;
        private readonly List<float> ayList;
        private readonly List<float> vxList;
        private readonly List<float> vyList;

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

        public List<double> A { get; set; }
        public List<double> Time { get; set; }
        public List<double> V { get; set; }
        public List<double> X { get; set; }
        public List<double> Y { get; set; }

        public List<double> NormList(List<double> list, double min, double max)
        {
            if (min == max)
                return list.Select(t => min).ToList();
            List<double> l = new List<double>(list);
            _ = l.RemoveAll(item => double.IsNaN(item));
            List<double> returnList = list.Select(t => (t - l.Min()) / (l.Max() - l.Min()) * (max - min) + min).ToList();
            return returnList;
        }
    }
}