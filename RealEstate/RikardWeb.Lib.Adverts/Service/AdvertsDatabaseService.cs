using Microsoft.Extensions.Options;
using RikardLib.AspLog;
using RikardWeb.Lib.Adverts.Options;
using RikardWeb.Lib.Db.Service;
using System;
using System.Collections.Generic;
using System.Text;
using RikardWeb.Lib.Adverts.DbModels;
using System.Threading.Tasks;

namespace RikardWeb.Lib.Adverts.Service
{
    public class AdvertsDatabaseService : IAdvertsDatabaseService
    {
        private readonly IAspLogger logger;
        private readonly AdvertsDatabase advertsDatabase;
        private readonly IOptions<AdvertsOptions> advertsOptions;

        public AdvertsDatabaseService(IAspLogger logger, IMongoDbService mongoDbService, IOptions<AdvertsOptions> advertsOptions)
        {
            this.logger = logger;

            var databaseFactory = mongoDbService.GetDatabase();
            this.advertsDatabase = AdvertsDatabase.Init(databaseFactory, advertsOptions);
            this.advertsOptions = advertsOptions;

            logger.Info("AdvertsDatabaseService has been initialized.");

            logger.Info($"Adverts directories: RootDirectory={advertsOptions.Value.RootDirectory}, NfsDirectory={advertsOptions.Value.NfsDirectory}, FilesDirectory={advertsOptions.Value.FilesDirectory}");
        }

        public Task<AdRentLotModel> GetAdRentLotById(string id)
        {
            return advertsDatabase.GetAdRentLotById(id);
        }

        public Task<AdRentLotModel> GetAdRentLotByIdTryArch(string id)
        {
            return advertsDatabase.GetAdRentLotByIdTryArch(id);
        }

        public Task<List<AdRentLotModel>> GetAdRentLots(AdvRentSearchModel searchModel, int skip = 0, int limit = 0)
        {
            return advertsDatabase.GetAdRentLots(searchModel, skip, limit);
        }

        public Task<List<AdRentLotModel>> GetNearestAdRentLot(AdRentLotModel adv, int limit = 3)
        {
            return advertsDatabase.GetNearestAdRentLot(adv, limit);
        }

        public Task<List<AdRentLotModel>> GetSimilarAdRentLot(AdRentLotModel adv, int limit = 3)
        {
            return advertsDatabase.GetSimilarAdRentLot(adv, limit);
        }

        public void RunAdvertsUpdater()
        {
            advertsDatabase.RunAdvertsUpdater();
        }
    }
}
