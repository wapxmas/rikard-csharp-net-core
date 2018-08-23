using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Adverts.DbModels
{
    public class AddrStreetModel
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Code { get; set; }
        public string FormalName { get; set; }
        public string FormalNameNorm { get; set; }
        public string OffName { get; set; }
        public string ShortName { get; set; }
        [BsonIgnoreIfNull]
        public string Selection { get; set; }
        [BsonIgnoreIfNull]
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> Centroid { get; set; }
        [BsonIgnoreIfNull]
        public int? Rank { get; set; }
    }
}
