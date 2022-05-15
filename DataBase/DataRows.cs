﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.DataBase
{

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
            return list.Select(t => Norm(t, min, max, list.Min(), list.Max())).ToList();
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
}
