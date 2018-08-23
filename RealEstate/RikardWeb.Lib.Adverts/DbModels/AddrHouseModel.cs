using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using RikardWeb.Lib.Adverts.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Adverts.DbModels
{
    public class AddrHouseModel
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string StreetId { get; set; }
        public string HouseNum { get; set; }
        public string BuildNum { get; set; }
        public string StrucNum { get; set; }
        public string StrStatus { get; set; }
        public string IFNSFL { get; set; }
        public string IFNSUL { get; set; }
        public string OKATO { get; set; }
        public string OKTMO { get; set; }
        public string PostalCode { get; set; }
        [BsonIgnoreIfNull]
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> Centroid { get; set; }
        [BsonIgnoreIfNull]
        public GisAttributes GisAttributes { get; set; }
        [BsonIgnoreIfNull]
        public string DetailsUrl { get; set; }
        [BsonIgnoreIfNull]
        public List<Details> Details { get; set; }
    }
}
