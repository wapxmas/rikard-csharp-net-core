using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Db
{
    public class MongoDbCredentials
    {
        public string User { get; private set; }
        public string Password { get; private set; }
        public string Database { get; private set; }
        public string Host { get; private set; }

        public string ConnectionString
        {
            get
            {
                return $"mongodb://{User}:{Password}@{Host}/{Database}";
            }
        }

        public MongoDbCredentials(string user, string password, string database, string host)
        {
            this.User = user;
            this.Password = password;
            this.Database = database;
            this.Host = host;
        }
    }
}
