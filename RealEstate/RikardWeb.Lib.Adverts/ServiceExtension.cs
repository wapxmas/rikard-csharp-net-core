using RikardWeb.Lib.Adverts.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddAdvertsDatabaseService(this IServiceCollection service)
            => service.AddSingleton<IAdvertsDatabaseService, AdvertsDatabaseService>();
    }
}
