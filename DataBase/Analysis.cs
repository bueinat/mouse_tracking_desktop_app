using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace mouse_tracking_web_app.DataBase
{
    public class Analysis
    {
        [BsonElement("_id")]
        public ObjectId ID { get; set; }

        [BsonElement("timestep")]
        public List<int> TimeStep { get; set; }

        [BsonElement("x")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public List<float> X { get; set; }

        [BsonElement("y")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public List<float> Y { get; set; }

        [BsonElement("vx")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public List<float> VelocityX { get; set; }

        [BsonElement("vy")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public List<float> VelocityY { get; set; }

        [BsonElement("ax")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public List<float> AccelerationX { get; set; }

        [BsonElement("ay")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
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

        public DataTable AnalysisDataTable => AnalysisToDataTable();

        public string GetCSVString(string filePath)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = AnalysisDataTable.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            _ = sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in AnalysisDataTable.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                _ = sb.AppendLine(string.Join(",", fields));
            }

            return sb.ToString().Replace("@WORKING_PATH", filePath);
        }

        private DataTable AnalysisToDataTable()
        {
            DataTable dataTable = new DataTable();
            _ = dataTable.Columns.Add("TimeStep", typeof(int));
            _ = dataTable.Columns.Add("X", typeof(float));
            _ = dataTable.Columns.Add("Y", typeof(float));
            _ = dataTable.Columns.Add("VelocityX", typeof(float));
            _ = dataTable.Columns.Add("VelocityY", typeof(float));
            _ = dataTable.Columns.Add("AccelerationX", typeof(float));
            _ = dataTable.Columns.Add("AccelerationY", typeof(float));
            _ = dataTable.Columns.Add("Curviness", typeof(float));
            _ = dataTable.Columns.Add("Path", typeof(string));
            _ = dataTable.Columns.Add("IsSniffing", typeof(bool));
            _ = dataTable.Columns.Add("IsDrinking", typeof(bool));
            _ = dataTable.Columns.Add("IsNoseCasting", typeof(bool));


            // TODO: try to make it more general
            for (int i = 0; i < TimeStep.Count; i++)
            {
                DataRow dr = dataTable.NewRow();
                dr["TimeStep"] = TimeStep[i];
                dr["X"] = X[i];
                dr["Y"] = Y[i];
                dr["VelocityX"] = VelocityX[i];
                dr["VelocityY"] = VelocityY[i];
                dr["AccelerationX"] = AccelerationX[i];
                dr["AccelerationY"] = AccelerationY[i];
                dr["Curviness"] = Curviness[i];
                dr["Path"] = Path[i];
                dr["IsSniffing"] = IsSniffing[i];
                dr["IsDrinking"] = IsDrinking[i];
                dr["IsNoseCasting"] = IsNoseCasting[i];

                dataTable.Rows.Add(dr);
            }

            return dataTable;
        }

        public Dictionary<string, List<Tuple<int, int>>> GetFeaturesTimes(List<string> featuresNames)
        {
            Dictionary<string, List<Tuple<int, int>>> featuresTimes = new Dictionary<string, List<Tuple<int, int>>>();
            bool isOpen;
            int openingIndex;
            int closingIndex;
            int i;

            foreach (string name in featuresNames)
            {
                featuresTimes.Add(name, new List<Tuple<int, int>>());
                List<bool> featureList = (List<bool>)GetType().GetProperty(name).GetValue(this);

                isOpen = false;
                openingIndex = 0;
                closingIndex = 0;
                i = 0;
                while (i < featureList.Count)
                {
                    if (!isOpen)
                    {
                        isOpen = featureList[i];

                        if (isOpen)
                        {
                            openingIndex = i;
                            closingIndex = i + 1;
                        }
                        i++;
                    }
                    else
                    {
                        if (featureList[i])
                        {
                            closingIndex++;
                            i++;
                        }
                        else
                        {
                            featuresTimes[name].Add(new Tuple<int, int>(openingIndex, closingIndex));
                            isOpen = false;
                        }
                    }
                }
                if (isOpen)
                    featuresTimes[name].Add(new Tuple<int, int>(openingIndex, closingIndex));
            }

            return featuresTimes;
        }
    }
}