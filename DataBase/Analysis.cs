using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

// TODO: set names properly

namespace mouse_tracking_web_app.DataBase
{
    public class Analysis
    {
        [BsonElement("_id")]
        public ObjectId ID { get; set; }

        [BsonElement("timestep")]
        public List<int> TimeStep { get; set; }

        [BsonElement("x")]
        public List<float> X { get; set; }

        [BsonElement("y")]
        public List<float> Y { get; set; }

        [BsonElement("vx")]
        public List<float> VelocityX { get; set; }

        [BsonElement("vy")]
        public List<float> VelocityY { get; set; }

        [BsonElement("ax")]
        public List<float> AccelerationX { get; set; }

        [BsonElement("ay")]
        public List<float> AccelerationY { get; set; }

        [BsonElement("curviness")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public List<float> Curviness { get; set; }

        [BsonElement("path")]
        public List<string> Path { get; set; }

        [BsonElement("is_sniffing")]
        public List<bool> IsSniffing { get; set; }

        [BsonElement("is_drinking")]
        public List<bool> IsDrinking { get; set; }

        [BsonElement("is_nose_casting")]
        public List<bool> IsNoseCasting { get; set; }

        [BsonElement("video")]
        public ObjectId Video { get; set; }

        private readonly List<string> featuresNames = new List<string>
            {
                "IsDrinking",
                "IsNoseCasting",
                "IsSniffing"
            };

        public List<List<string>> GetFeaturesSubsets()
        {
            List<List<string>> list = new List<List<string>>();

            Console.WriteLine(string.Join(", ", featuresNames.GetRange(0, 1)));
            Console.WriteLine(string.Join(", ", featuresNames.GetRange(1, 1)));
            Console.WriteLine(string.Join(", ", featuresNames.GetRange(1, 2)));

            for (int index = 0; index < featuresNames.Count; index++)
            {
                for (int count = 1; count <= featuresNames.Count - index; count++)
                {
                    Console.WriteLine($"({index}, {count})");
                    list.Add(featuresNames.GetRange(index, count));
                }
            }
            return list;
        }


        private List<string> GetFeaturesAtStep(List<List<bool>> featuresColumns, int i)
        {
            List<string> currentFeatures = new List<string>();
            foreach (List<bool> featureList in featuresColumns)
            {
                if (featureList[i])
                    currentFeatures.Add(featuresNames[featuresColumns.IndexOf(featureList)]);
            }
            return currentFeatures;
        }

        public Dictionary<Tuple<int, int>, List<string>> GetFeaturesTimes()
        {
            Dictionary<Tuple<int, int>, List<string>> featuresTimes = new Dictionary<Tuple<int, int>, List<string>>();
            List<List<bool>> featuresColumns = new List<List<bool>>
            {
                IsDrinking,
                IsNoseCasting,
                IsSniffing
            };
            List<string> currentFeatures = new List<string>();
            List<string> prevFeatures;

            bool isOpen = false;
            int openingIndex = 0;
            int closingIndex = 0;
            int i = 0;
            while (i < IsSniffing.Count)
            {
                if (!isOpen)
                {
                    foreach (List<bool> featureList in featuresColumns)
                        isOpen = isOpen || featureList[i];

                    if (isOpen)
                    {
                        openingIndex = i;
                        closingIndex = i + 1;
                        currentFeatures = GetFeaturesAtStep(featuresColumns, i);
                    }
                    i++;
                }
                else
                {
                    prevFeatures = new List<string>(currentFeatures);
                    currentFeatures = GetFeaturesAtStep(featuresColumns, i);
                    bool isEqual = Enumerable.SequenceEqual(currentFeatures, prevFeatures);
                    //if (currentFeatures.Equals(prevFeatures))
                    if (Enumerable.SequenceEqual(currentFeatures, prevFeatures))
                    {
                        closingIndex++;
                        i++;
                    }
                    else
                    {
                        featuresTimes.Add(new Tuple<int, int>(openingIndex, closingIndex), currentFeatures);
                        isOpen = false;
                    }
                }
            }
            if (isOpen)
                featuresTimes.Add(new Tuple<int, int>(openingIndex, closingIndex), currentFeatures);
            return featuresTimes;
        }
    }
}