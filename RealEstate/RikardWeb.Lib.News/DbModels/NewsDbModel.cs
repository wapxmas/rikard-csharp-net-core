using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RikardWeb.Lib.News.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.News.DbModels
{
    public class NewsDbModel
    {
        public ObjectId Id { get; set; }
        public string Guid { get; set; }
        [BsonIgnoreIfNull]
        public string Header { get; set; }
        public string Text { get; set; }
        [BsonIgnoreIfNull]
        public string Html { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Date { get; set; }
        public string Url { get; set; }
        [BsonIgnoreIfNull]
        public Tweet Tweet { get; set; }
        [BsonIgnoreIfNull]
        public List<SimilarArticle> Similar { get; set; }
        [BsonIgnoreIfNull]
        public List<string> Tags { get; set; }
        [BsonIgnoreIfNull]
        public List<ArticleWordWeight> WW { get; set; }
    }
}
