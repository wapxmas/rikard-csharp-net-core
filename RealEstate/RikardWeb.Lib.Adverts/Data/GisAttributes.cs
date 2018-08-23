using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Adverts.Data
{
    public class GisAttributes
    {
        [BsonIgnoreIfNull]
        public int? Rank;
        [BsonIgnoreIfNull]
        public string Purpose;
        [BsonIgnoreIfNull]
        public int? FirmCount;
        [BsonIgnoreIfNull]
        [BsonIgnoreIfDefault]
        public string BuildingName;
        [BsonIgnoreIfNull]
        public string PostalIndex;
        [BsonIgnoreIfNull]
        public int? Elevation;
        [BsonIgnoreIfNull]
        public string Synonym;
        [BsonIgnoreIfNull]
        public string Info;
    }
}
