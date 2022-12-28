using System;
using System.Collections.Generic;
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
            FeaturesDictionary = CreateFeaturesDictionary();

            FeaturesPercents = new Dictionary<string, double>();
            foreach (var item in FeaturesDictionary)
                FeaturesPercents[item.Key] = item.Value.Sum(x => Convert.ToInt32(x)) / (double)NSteps * 100;
        }

        public double AverageAcceleration => DataRows.A.Average();

        public double AverageSpeed => DataRows.V.Average();

        public DataRows DataRows { get; }

        public Dictionary<string, List<bool>> FeaturesDictionary { get; }

        public Dictionary<string, double> FeaturesPercents { get; }

        public int NSteps => analysis.TimeStep.Count;

        public double TotalDistance => DataRows.V.Sum();

        private Dictionary<string, List<bool>> CreateFeaturesDictionary()
        {
            Dictionary<string, List<bool>> featuresDictionary = new Dictionary<string, List<bool>>();

            foreach (var item in analysis.Features[0])
                featuresDictionary.Add(item.Key, new List<bool>());

            foreach (Dictionary<string, bool> dict in analysis.Features)
            {
                foreach (var feat in dict)
                    featuresDictionary[feat.Key].Add(feat.Value);
            }
            return featuresDictionary;
        }
    }
}