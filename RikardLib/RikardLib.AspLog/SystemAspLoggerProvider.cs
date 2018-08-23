using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardLib.AspLog
{
    class SystemAspLoggerProvider : ILoggerProvider
    {
        private readonly bool debugInfo;

        public SystemAspLoggerProvider(bool debugInfo)
        {
            this.debugInfo = debugInfo;
        }

        public ILogger CreateLogger(string categoryName) => new SystemAspLogger(debugInfo);

        public void Dispose()
        {

        }
    }
}
