using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RikardWeb.Lib.News.DbModels;
using RikardLib.AspLog;
using Microsoft.Extensions.Options;
using RikardWeb.Lib.Db;
using RikardWeb.Lib.News;
using RikardWeb.Lib.Db.Options;
using RikardWeb.Lib.Db.Service;

namespace RikardWeb.Lib.News.Services
{
    public class NewsDatabaseService : INewsDatabaseService
    {
        private readonly IAspLogger logger;
        private readonly NewsDatabase newsDatabase;

        public NewsDatabaseService(IAspLogger logger, IMongoDbService mongoDbService)
        {
            this.logger = logger;

            var databaseFactory = mongoDbService.GetDatabase();
            this.newsDatabase = NewsDatabase.Init(databaseFactory);

            logger.Info("NewsDatabaseService has been initialized.");
        }

        public async Task<IEnumerable<NewsDbModel>> GetLatestNewsArticles(int count, int skip)
        {
            return await newsDatabase.GetLatestNewsArticles(count, skip);
        }

        public async Task<NewsDbModel> GetNewsArticleByGuid(string guid)
        {
            return await newsDatabase.GetNewsArticleByGuid(guid);
        }

        public async Task<long> GetTotalNewsNumber()
        {
            return await newsDatabase.GetTotalNewsNumber();
        }
    }
}
