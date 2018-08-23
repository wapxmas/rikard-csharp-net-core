using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Adverts.DbModels
{
    public class GeoRingModel
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        [BsonIgnoreIfNull]
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> Centroid { get; set; }
        [BsonIgnoreIfNull]
        public GeoJsonPolygon<GeoJson2DGeographicCoordinates> Polygon { get; set; }
    }
}
