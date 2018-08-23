using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace RikardLib.AspLog
{
    class SystemAspLogger : ILogger
    {
        private readonly AspLogger aspLogger;
        private readonly bool debugInfo;

        public SystemAspLogger(bool debugInfo)
        {
            this.debugInfo = debugInfo;
            aspLogger = new AspLogger();
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if(formatter != null)
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        if(debugInfo)
                        {
                            aspLogger.Debug(formatter(state, exception));
                        }
                        break;
                    case LogLevel.Warning:
                        aspLogger.Warn(formatter(state, exception));
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        aspLogger.Error(formatter(state, exception));
                        break;
                    default:
                        aspLogger.Info(formatter(state, exception));
                        break;
                }
            }
        }
    }
}
