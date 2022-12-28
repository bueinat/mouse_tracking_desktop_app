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
            FeaturesTimes = CreateFeaturesTimes();

            FeaturesPercents = new Dictionary<string, double>();
            foreach (KeyValuePair<string, List<bool>> item in FeaturesDictionary)
                FeaturesPercents[item.Key] = item.Value.Sum(x => Convert.ToInt32(x)) / (double)NSteps * 100;
        }

        public double AverageAcceleration => DataRows.A.Average();

        public double AverageSpeed => DataRows.V.Average();

        public DataRows DataRows { get; }

        public Dictionary<string, List<bool>> FeaturesDictionary { get; }
        public Dictionary<string, List<Tuple<int, int>>> FeaturesTimes { get; }

        public Dictionary<string, double> FeaturesPercents { get; }

        public int NSteps => analysis.TimeStep.Count;

        public double TotalDistance => DataRows.V.Sum();

        private Dictionary<string, List<bool>> CreateFeaturesDictionary()
        {
            Dictionary<string, List<bool>> featuresDictionary = new Dictionary<string, List<bool>>();

            foreach (KeyValuePair<string, bool> item in analysis.Features[0])
                featuresDictionary.Add(item.Key, new List<bool>());

            foreach (Dictionary<string, bool> dict in analysis.Features)
            {
                foreach (KeyValuePair<string, bool> feat in dict)
                    featuresDictionary[feat.Key].Add(feat.Value);
            }
            return featuresDictionary;
        }

        private Dictionary<string, List<Tuple<int, int>>> CreateFeaturesTimes()
        {
            Dictionary<string, List<Tuple<int, int>>> featuresTimes = new Dictionary<string, List<Tuple<int, int>>>();

            bool isOpen;
            int openingIndex;
            int closingIndex;
            int i;

            foreach (KeyValuePair<string, List<bool>> features_dictionary_item in FeaturesDictionary)
            {
                featuresTimes.Add(features_dictionary_item.Key, new List<Tuple<int, int>>());

                isOpen = false;
                openingIndex = 0;
                closingIndex = 0;
                i = 0;
                while (i < features_dictionary_item.Value.Count)
                {
                    if (!isOpen)
                    {
                        isOpen = features_dictionary_item.Value[i];

                        if (isOpen)
                        {
                            openingIndex = i;
                            closingIndex = i + 1;
                        }
                        i++;
                    }
                    else
                    {
                        if (features_dictionary_item.Value[i])
                        {
                            closingIndex++;
                            i++;
                        }
                        else
                        {
                            featuresTimes[features_dictionary_item.Key].Add(new Tuple<int, int>(openingIndex, closingIndex));
                            isOpen = false;
                        }
                    }
                }
                if (isOpen)
                    featuresTimes[features_dictionary_item.Key].Add(new Tuple<int, int>(openingIndex, closingIndex));
            }

            return featuresTimes;
        }
    }
}