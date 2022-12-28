using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
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

        [BsonElement("features")]
        public List<Dictionary<string, bool>> Features { get; set; }

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
            //_ = dataTable.Columns.Add("Features", typeof(bool));
            _ = dataTable.Columns.Add("Features", typeof(Dictionary<string, bool>));

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
                dr["Features"] = Features[i];

                dataTable.Rows.Add(dr);
            }

            return dataTable;
        }

        public Dictionary<string, List<bool>> FeaturesDictionary()
        {
            Dictionary<string, List<bool>> featuresDictionary = new Dictionary<string, List<bool>>();

            foreach (KeyValuePair<string, bool> item in Features[0])
                featuresDictionary.Add(item.Key, new List<bool>());

            foreach (Dictionary<string, bool> dict in Features)
            {
                foreach (KeyValuePair<string, bool> feat in dict)
                    featuresDictionary[feat.Key].Add(feat.Value);
            }
            return featuresDictionary;
        }
    }
}