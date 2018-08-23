using Microsoft.Extensions.DependencyInjection;
using RikardWeb.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServicesExtension
    {
        public static IServiceCollection AddSendEmailService(this IServiceCollection service)
            => service.AddSingleton<ISendEmailService, SendEmailService>();
    }
}
