using RikardWeb.Lib.Db.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServicesExtension
    {
        public static IServiceCollection AddMongoDbService(this IServiceCollection service)
            => service.AddSingleton<IMongoDbService, MongoDbService>();
    }
}
