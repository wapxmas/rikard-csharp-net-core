using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Adverts.DbModels
{
    public class STStationsModel
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name;
        public string StationName;
        public string RouteNumbers;
        public string Direction;
        [BsonIgnoreIfNull]
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> GeoPoint { get; set; }
    }
}
