using Microsoft.Extensions.DependencyInjection;
using RikardLib.AspHelpers.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddViewRender(this IServiceCollection service)
            => service.AddScoped<IViewRenderService, ViewRenderService>();
    }
}
