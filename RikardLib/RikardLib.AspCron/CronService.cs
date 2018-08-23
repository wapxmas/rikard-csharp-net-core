using System;
using System.Collections.Generic;
using System.Text;

namespace RikardLib.AspCron
{
    public class CronService : ICronService
    {
        private readonly CronDaemon daemon;

        public CronService()
        {
            daemon = new CronDaemon();
        }

        public void AddJob(string schedule, Action action, string actionName)
        {
            daemon.AddJob(schedule, new CronAction(action, actionName));
        }
    }
}
