using SmsRu;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddSmsRu(this IServiceCollection service)
            => service.AddSingleton<ISmsRuService, SmsRuService>();
    }
}
