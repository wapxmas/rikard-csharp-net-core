using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Adverts.Data
{
    public class Details
    {
        [BsonIgnoreIfNull]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<string, string> Main;
        [BsonIgnoreIfNull]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<string, List<List<string>>> Extra;
        [BsonIgnoreIfNull]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<string, List<string>> Stewardship;
        [BsonIgnoreIfNull]
        public string StewardshipInfo;
    }
}
