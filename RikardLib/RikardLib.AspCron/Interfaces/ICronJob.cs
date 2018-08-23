using System;
using System.Collections.Generic;
using System.Text;

namespace RikardLib.AspCron
{
    public interface ICronJob
    {
        void Execute(DateTime date_time);
    }
}
