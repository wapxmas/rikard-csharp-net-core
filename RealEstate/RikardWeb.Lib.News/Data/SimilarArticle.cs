using MongoDB.Bson.Serialization.Attributes;
using RikardWeb.Lib.News.DbModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.News.Data
{
    public class SimilarArticle
    {
        [BsonIgnore]
        public NewsDbModel Article;
        [BsonIgnore]
        public double[] Weights;
        public double Similarity;
        public string Guid;
    }
}
