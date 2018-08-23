using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RikardLib.AspLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddAspLogger(this IServiceCollection service)
            => service.AddSingleton<IAspLogger>(new AspLogger());

        public static ILoggerFactory AddAspLogger(this ILoggerFactory logger, bool debugInfo = false)
        {
            logger.AddProvider(new SystemAspLoggerProvider(debugInfo));
            return logger;
        }
    }
}
