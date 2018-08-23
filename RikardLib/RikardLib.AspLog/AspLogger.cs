using System;
using RikardLib.Log;
using System.Runtime.CompilerServices;

namespace RikardLib.AspLog
{
    public class AspLogger : IAspLogger
    {
        private readonly Logger logger;

        public AspLogger()
        {
            logger = new Logger();
        }

        public void Debug(object message) => logger.Debug(message, showCaller: false);

        public void Debug(object message, Exception exception) => logger.Debug(message, exception, showCaller: false);

        public void Error(object message) => logger.Error(message, showCaller: false);

        public void Error(object message, Exception exception) => logger.Error(message, exception, showCaller: false);

        public void Fatal(object message) => logger.Fatal(message, showCaller: false);

        public void Fatal(object message, Exception exception) => logger.Fatal(message, exception, showCaller: false);

        public void Info(object message) => logger.Info(message, showCaller: false);

        public void Info(object message, Exception exception) => logger.Info(message, exception, showCaller: false);

        public void Warn(object message) => logger.Warn(message, showCaller: false);

        public void Warn(object message, Exception exception) => logger.Warn(message, exception, showCaller: false);
    }
}
