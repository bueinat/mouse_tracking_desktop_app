using System;
using System.Linq;

namespace mouse_tracking_web_app.DataBase
{
    public class AnalysisStats
    {
        private readonly Analysis analysis;

        public AnalysisStats(Analysis analysis)
        {
            this.analysis = analysis;
            DataRows = new DataRows(analysis);
        }

        public double AverageAcceleration => DataRows.A.Average();

        public double AverageSpeed => DataRows.V.Average();

        //public readonly DataRows dataRows;
        public DataRows DataRows { get; }

        public double IsDrinkingPercent => analysis.IsDrinking.Sum(x => Convert.ToInt32(x)) / (double)NSteps * 100;
        public double IsNoseCastingPercent => analysis.IsNoseCasting.Sum(x => Convert.ToInt32(x)) / (double)NSteps * 100;
        public double IsSniffingPercent => analysis.IsSniffing.Sum(x => Convert.ToInt32(x)) / (double)NSteps * 100;
        public int NSteps => analysis.TimeStep.Count;
        public double TotalDistance => DataRows.V.Sum();
    }
}