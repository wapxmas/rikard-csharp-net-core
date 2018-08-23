using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RikardLib.AspCron
{
    public class CronJob : ICronJob
    {
        private readonly ICronSchedule cronSchedule = new CronSchedule();
        private readonly CronAction cronAction;
        private Thread thread;

        public CronJob(string schedule, CronAction action)
        {
            this.cronSchedule = new CronSchedule(schedule);
            this.cronAction = action;
            thread = new Thread(cronAction.Action);
        }

        private object _lock = new object();

        public void Execute(DateTime date_time)
        {
            lock (_lock)
            {
                if (!cronSchedule.IsTime(date_time))
                    return;

                if (thread.ThreadState == ThreadState.Running)
                    return;

                thread = new Thread(cronAction.Action);
                thread.Start();
            }
        }
    }
}
