using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Adverts.DbModels
{
    public class GeoAoModel
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Abbr { get; set; }
        public GeoJsonMultiPolygon<GeoJson2DGeographicCoordinates> Polygon { get; set; }
    }
}
