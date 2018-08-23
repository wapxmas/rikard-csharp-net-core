using Microsoft.Extensions.DependencyInjection;
using RikardLib.AspCron;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddCron(this IServiceCollection service)
            => service.AddSingleton<ICronService, CronService>();
    }
}
