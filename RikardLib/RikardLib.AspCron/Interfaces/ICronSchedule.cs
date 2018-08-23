using System;
using System.Collections.Generic;
using System.Text;

namespace RikardLib.AspCron
{
    interface ICronSchedule
    {
        bool IsValid(string expression);
        bool IsTime(DateTime date_time);
    }
}
