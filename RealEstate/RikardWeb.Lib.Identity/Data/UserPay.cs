using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RikardWeb.Lib.Identity.Data
{
    public class UserPay
    {
        public double Amount { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Date { get; set; } = DateTime.Now;
    }
}