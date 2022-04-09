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

        public List<int> timestep { get; set; }
        public List<float> x { get; set; }
        public List<float> y { get; set; }
        public List<float> vx { get; set; }
        public List<float> vy { get; set; }
        public List<float> ax { get; set; }
        public List<float> ay { get; set; }

        [BsonElement("curviness")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public List<float> Curviness { get; set; }

        public List<string> path { get; set; }
        public List<bool> is_sniffing { get; set; }
        public List<bool> is_drinking { get; set; }
        public List<bool> is_nose_casting { get; set; }

        [BsonElement("video")]
        public ObjectId Video { get; set; }
    }
}