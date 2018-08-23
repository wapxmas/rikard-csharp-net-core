using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RikardLib.AspCron
{
    public interface ICronDaemon
    {
        void AddJob(string schedule, CronAction action);
    }
}
