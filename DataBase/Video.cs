﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace mouse_tracking_web_app.DataBase
{
    public class Video
    {
        [BsonElement("_id")]
        public ObjectId ID { get; set; }

        [BsonElement("registration_date")]
        public DateTime RegisteredDate { get; set; }

        [BsonElement("modification_date")]
        public DateTime ModificationDate { get; set; }

        [BsonElement("name")]
        [BsonRepresentation(BsonType.String)]
        public string Name { get; set; }

        [BsonElement("nframes")]
        public int NFrames { get; set; }

        [BsonElement("length")]
        public string Length { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("link_to_data")]
        public string LinkToData { get; set; }

        [BsonElement("analysis")]
        public ObjectId Analysis { get; set; }
    }
}