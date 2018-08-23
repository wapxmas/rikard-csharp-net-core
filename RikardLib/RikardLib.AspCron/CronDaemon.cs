using RikardLib.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RikardLib.AspCron
{
    public class CronDaemon : ICronDaemon
    {
        private readonly Timer timer;
        private readonly ConcurrentBag<ICronJob> cron_jobs = new ConcurrentBag<ICronJob>();
        private DateTime _last = DateTime.Now;
        private readonly Logger logger = new Logger();

        private const int TIMER_INTERVAL = 30000;

        public CronDaemon()
        {
            timer = new Timer(OnTimer, null, TIMER_INTERVAL, TIMER_INTERVAL);
            logger.Info("Cron daemon have been started.");
        }

        public void AddJob(string schedule, CronAction action)
        {
            var cj = new CronJob(schedule, action);
            cron_jobs.Add(cj);
        }

        private void OnTimer(object state)
        {
            if (DateTime.Now.Minute != _last.Minute)
            {
                _last = DateTime.Now;
                foreach (ICronJob job in cron_jobs)
                    job.Execute(DateTime.Now);
            }
        }
    }
}
