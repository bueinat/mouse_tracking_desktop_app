using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

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
    }
}