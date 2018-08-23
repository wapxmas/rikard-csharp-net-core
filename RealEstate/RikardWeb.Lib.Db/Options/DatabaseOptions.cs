using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Lib.Db.Options
{
    public class DatabaseOptions
    {
        public MongoDb MongoDb { get; set; }
    }

    public class MongoDb
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string Host { get; set; }
    }
}
