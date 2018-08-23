using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RikardWeb.Lib.Identity
{
    public class IdentityRole
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; }
        
        public string NormalizedName { get; set; }
    }
}