using RikardWeb.Lib.News.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddNewsDatabaseService(this IServiceCollection service)
            => service.AddSingleton<INewsDatabaseService, NewsDatabaseService>();
    }
}
