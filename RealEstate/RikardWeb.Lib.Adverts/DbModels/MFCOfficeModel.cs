using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Adverts.DbModels
{
    public class MFCOfficeModel
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ShortName { get; set; }
        public string Address { get; set; }
        [BsonIgnoreIfNull]
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> GeoPoint { get; set; }
    }
}
