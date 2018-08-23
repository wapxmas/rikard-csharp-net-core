using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Identity.Data
{
    public class UserBalance
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ServiceExpired { get; set; }
        public double ServiceBalance { get; set; }
        [BsonIgnoreIfNull]
        public List<UserPay> Pays { get; set; }
    }
}
