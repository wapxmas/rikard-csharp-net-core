using System;
using System.Collections.Generic;
using System.Text;

namespace RikardLib.AspCron
{
    public interface ICronService
    {
        void AddJob(string schedule, Action action, string actionName);
    }
}
