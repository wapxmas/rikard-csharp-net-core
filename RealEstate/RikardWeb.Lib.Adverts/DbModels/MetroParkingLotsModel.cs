using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Adverts.DbModels
{
    public class MetroParkingLotsModel
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ParkingName { get; set; }
        public string LocationDescription { get; set; }
        public int CarCapacity { get; set; }
        [BsonIgnoreIfNull]
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> GeoPoint { get; set; }
    }
}
