using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Db.Service
{
    public interface IMongoDbService
    {
        DatabaseFactory GetDatabase();
    }
}
