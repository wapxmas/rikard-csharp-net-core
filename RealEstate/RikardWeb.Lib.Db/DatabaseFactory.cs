using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Db
{
    public class DatabaseFactory
    {
        private static volatile object _instanceLock = new object();

        private static DatabaseFactory _instance;

        public static DatabaseFactory Instance
        {
            get
            {
                return _instance;
            }
        }

        public readonly IMongoDatabase MongoDatabase;

        private readonly MongoClient mongoClient;

        private DatabaseFactory(MongoDbCredentials mongoCredentials)
        {
            this.mongoClient = new MongoClient(mongoCredentials.ConnectionString);
            this.MongoDatabase = mongoClient.GetDatabase(mongoCredentials.Database);
        }

        public static DatabaseFactory Init(MongoDbCredentials mongoCredentials)
        {
            if(_instance == null)
            {
                lock(_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new DatabaseFactory(mongoCredentials);
                    }
                }
            }

            return _instance;
        }
    }
}
