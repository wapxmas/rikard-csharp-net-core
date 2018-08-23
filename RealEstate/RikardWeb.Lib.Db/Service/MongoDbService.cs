using Microsoft.Extensions.Options;
using RikardLib.AspLog;
using RikardWeb.Lib.Db.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Db.Service
{
    public class MongoDbService : IMongoDbService
    {
        private readonly DatabaseFactory databaseFactory;

        public MongoDbService(IAspLogger logger, IOptions<DatabaseOptions> databaseOptions)
        {
            var mongoCredentials = new MongoDbCredentials(
                databaseOptions.Value.MongoDb.User,
                databaseOptions.Value.MongoDb.Password,
                databaseOptions.Value.MongoDb.Database,
                databaseOptions.Value.MongoDb.Host);

            this.databaseFactory = DatabaseFactory.Init(mongoCredentials);

            logger.Info("MongoDbService has been initialized.");
        }

        public DatabaseFactory GetDatabase()
        {
            return databaseFactory;
        }
    }
}
