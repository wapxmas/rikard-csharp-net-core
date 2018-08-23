using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Adverts.DbModels
{
    public class ParkingAutomatesModel
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Location { get; set; }
        public string NumberOfParkingMeter { get; set; }
        [BsonIgnoreIfNull]
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> GeoPoint { get; set; }
    }
}
