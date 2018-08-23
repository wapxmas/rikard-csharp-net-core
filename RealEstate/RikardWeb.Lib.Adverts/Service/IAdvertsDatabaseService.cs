using RikardWeb.Lib.Adverts.DbModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RikardWeb.Lib.Adverts.Service
{
    public interface IAdvertsDatabaseService
    {
        Task<List<AdRentLotModel>> GetAdRentLots(AdvRentSearchModel searchModel, int skip = 0, int limit = 0);
        Task<AdRentLotModel> GetAdRentLotById(string id);
        Task<AdRentLotModel> GetAdRentLotByIdTryArch(string id);
        Task<List<AdRentLotModel>> GetNearestAdRentLot(AdRentLotModel adv, int limit = 3);
        Task<List<AdRentLotModel>> GetSimilarAdRentLot(AdRentLotModel adv, int limit = 3);
        void RunAdvertsUpdater();
    }
}
