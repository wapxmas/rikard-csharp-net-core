using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.News.DbModels
{
    public class WordWeightDbModel
    {
        public ObjectId Id { get; set; }
        public string Word { get; set; }
        public double Weight { get; set; }
    }
}
