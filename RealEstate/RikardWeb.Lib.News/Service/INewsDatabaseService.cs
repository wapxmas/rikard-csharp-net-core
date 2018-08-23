using RikardWeb.Lib.News.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Lib.News.Services
{
    public interface INewsDatabaseService
    {
        Task<IEnumerable<NewsDbModel>> GetLatestNewsArticles(int count, int skip);
        Task<NewsDbModel> GetNewsArticleByGuid(string guid);
        Task<long> GetTotalNewsNumber();
    }
}
